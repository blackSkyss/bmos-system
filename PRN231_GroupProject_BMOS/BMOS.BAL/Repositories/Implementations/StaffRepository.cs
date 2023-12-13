using AutoMapper;
using BMOS.BAL.DTOs.FireBase;
using BMOS.BAL.DTOs.OrderDetails;
using BMOS.BAL.DTOs.Staffs;
using BMOS.BAL.Exceptions;
using BMOS.BAL.Helpers;
using BMOS.BAL.Repositories.Interfaces;
using BMOS.DAL.Enums;
using BMOS.DAL.Infrastructures;
using BMOS.DAL.Models;
using Microsoft.AspNetCore.Http;
using Quartz.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Repositories.Implementations
{
    public class StaffRepository : IStaffRepository
    {
        private UnitOfWork _unitOfWork;
        private IMapper _mapper;
        public StaffRepository(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = (UnitOfWork)unitOfWork;
            _mapper = mapper;
        }

        #region Get staffs
        public async Task<IEnumerable<GetStaffResponse>> GetStaffsAsync()
        {
            try
            {
                var staffs = await _unitOfWork.StaffDAO.GetStaffsAsync();
                return _mapper.Map<IEnumerable<GetStaffResponse>>(staffs);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }
        #endregion

        #region Get staff detail
        public async Task<GetStaffResponse> GetStaffDetailAsync(int id)
        {
            try
            {
                var staff = await _unitOfWork.StaffDAO.GetStaffDetailAsync(id);
                if (staff == null)
                {
                    throw new NotFoundException("Staff does not exist in the system.");
                }

                return _mapper.Map<GetStaffResponse>(staff);
            }
            catch (NotFoundException ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new NotFoundException(error);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }
        #endregion

        #region Create Staff
        public async Task<GetStaffResponse> CreateStaffAsync(PostStaffRequest request, FireBaseImage fireBaseImage)
        {
            try
            {
                #region Validation
                // Birthdate
                var validBirthDate = DateHelper.ValidationBirthDay(request.BirthDate);
                if (!string.IsNullOrWhiteSpace(validBirthDate))
                {
                    throw new BadRequestException(validBirthDate);
                }

                // Email exist
                var existedEmail = await _unitOfWork.AccountDAO.GetAccountByEmail(request.Account.Email);
                if (existedEmail != null)
                {
                    throw new BadRequestException("Email already existed in the system.");
                }

                // Phone exist
                var staffPhoneExist = await _unitOfWork.StaffDAO.GetStaffByPhoneAsync(request.Phone);
                if (staffPhoneExist != null)
                {
                    throw new BadRequestException("Phone already existed in the system.");
                }

                // IdentityNumber exist
                var staffIdentityExist = await _unitOfWork.StaffDAO.GetStaffByIdentityAsync(request.IdentityNumber);
                if (staffIdentityExist != null)
                {
                    throw new BadRequestException("Identity number already existed in the system.");
                }
                #endregion

                #region Business logic
                Staff staff = new Staff();
                staff = _mapper.Map<Staff>(request);

                // Account
                var roleStaff = await _unitOfWork.RoleDAO.GetRoleDetailAsync((int)RoleEnum.Role.STAFF);
                staff.Account.PasswordHash = StringHelper.EncryptData(request.Account.PasswordHash);
                staff.Account.Status = Convert.ToBoolean(AccountEnum.Status.ACTIVE);
                staff.Account.Role = roleStaff;

                // Staff
                FileHelper.SetCredentials(fireBaseImage);
                FileStream fileStream = FileHelper.ConvertFormFileToStream(request.Avatar);
                Tuple<string, string> result = await FileHelper.UploadImage(fileStream, "Staff");
                staff.Avatar = result.Item1;
                staff.AvatarID = result.Item2;
                staff.RegisteredDate = DateTime.Today;


                await _unitOfWork.StaffDAO.CreateStaffAsync(staff);
                await _unitOfWork.CommitAsync();
                return _mapper.Map<GetStaffResponse>(staff);
                #endregion

            }
            catch (BadRequestException ex)
            {
                string fieldNameError = "";
                if(ex.Message.ToLower().Contains("age") || ex.Message.ToLower().Contains("birthday"))
                {
                    fieldNameError = "BirthDate";
                }else if(ex.Message.ToLower().Contains("email"))
                {
                    fieldNameError = "Email";
                }else if(ex.Message.ToLower().Contains("phone"))
                {
                    fieldNameError = "Phone";
                }else if(ex.Message.ToLower().Contains("identity number"))
                {
                    fieldNameError = "IdentityNumber";
                }
                string error = ErrorHelper.GetErrorString(fieldNameError, ex.Message);
                throw new BadRequestException(error);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString("Exception", ex.Message);
                throw new Exception(error);
            }
        }
        #endregion

        #region Delete Staff
        public async Task DeteleStaffAsync(int id)
        {
            try
            {
                var staff = await _unitOfWork.StaffDAO.GetStaffDetailAsync(id);
                if (staff == null)
                {
                    throw new NotFoundException("Staff does not exist.");
                }

                if (staff.Account.Status == Convert.ToBoolean(AccountEnum.Status.INACTIVE))
                {
                    throw new BadRequestException("Staff has already been deleted.");
                }
                staff.Account.Status = Convert.ToBoolean(AccountEnum.Status.INACTIVE);
                staff.QuitDate = DateTime.Today;

                _unitOfWork.StaffDAO.UpdateStaff(staff); 
                await _unitOfWork.CommitAsync();
            }
            catch (NotFoundException ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new NotFoundException(error);
            }
            catch (BadRequestException ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new BadRequestException(error);
            }
            catch (Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }
        #endregion

        public async Task<GetStaffResponse> UpdateStaffAsync(int staffId, UpdateStaffRequest request, FireBaseImage fireBaseImage)
        {
            try
            {
                var staff = await _unitOfWork.StaffDAO.GetStaffDetailAsync(staffId);
                if (staff == null)
                {
                    throw new NotFoundException("Staff does not exist in the system.");
                }
                // Birthdate
                var validBirthDate = DateHelper.ValidationBirthDay(request.BirthDate);
                if (!string.IsNullOrWhiteSpace(validBirthDate))
                {
                    throw new BadRequestException(validBirthDate);
                }

                // Phone exist
                var staffPhoneExist = await _unitOfWork.StaffDAO.GetStaffByPhoneAsync(request.Phone);
                if (staffPhoneExist != null && staffPhoneExist.AccountID != staffId)
                {
                    throw new BadRequestException("Phone already existed in the system.");
                }
                if(request.Avatar != null)
                {
                    FileHelper.SetCredentials(fireBaseImage);
                    await FileHelper.DeleteImageAsync(staff.AvatarID, "Staff");
                    FileStream fileStream = FileHelper.ConvertFormFileToStream(request.Avatar);
                    Tuple<string, string> result = await FileHelper.UploadImage(fileStream, "Staff");
                    staff.Avatar = result.Item1;
                    staff.AvatarID = result.Item2;
                }
                staff.FullName = request.FullName;
                staff.Address = request.Address;
                staff.Phone = request.Phone;
                staff.Gender = request.Gender;
                staff.BirthDate = request.BirthDate;
                if(request.PasswordHash.Equals("********") == false)
                {
                    staff.Account.PasswordHash = request.PasswordHash;
                }
                if(request.Status == true)
                {
                    staff.Account.Status = request.Status;
                    staff.QuitDate = null;
                } else
                {
                    staff.Account.Status = request.Status;
                    staff.QuitDate = DateTime.Today;
                }

                this._unitOfWork.StaffDAO.UpdateStaff(staff);
                await this._unitOfWork.CommitAsync();
                return this._mapper.Map<GetStaffResponse>(staff);
            } 
            catch(NotFoundException ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new NotFoundException(error);
            }
            catch(BadRequestException ex)
            {
                string fieldNameError = "";
                if (ex.Message.ToLower().Contains("phone"))
                {
                    fieldNameError = "Phone";
                } else if(ex.Message.ToLower().Contains("age") || ex.Message.ToLower().Contains("birthday"))
                {
                    fieldNameError = "Birthday";
                }
                string error = ErrorHelper.GetErrorString(fieldNameError, ex.Message);
                throw new BadRequestException(error);
            }
            catch(Exception ex)
            {
                string error = ErrorHelper.GetErrorString(ex.Message);
                throw new Exception(error);
            }
        }
    }
}
