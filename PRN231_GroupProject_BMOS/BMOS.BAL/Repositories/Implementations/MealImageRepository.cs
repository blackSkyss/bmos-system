using AutoMapper;
using BMOS.BAL.Repositories.Interfaces;
using BMOS.DAL.Infrastructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMOS.BAL.Repositories.Implementations
{
    public class MealImageRepository: IMealImageRepository
    {
        private UnitOfWork _unitOfWork;
        private IMapper _mapper;
        public MealImageRepository(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = (UnitOfWork)unitOfWork;
            _mapper = mapper;
        }
    }
}
