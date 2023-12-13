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
    public class ProductImageRepository: IProductImageRepository
    {
        private UnitOfWork _unitOfWork;
        private IMapper _mapper;
        public ProductImageRepository(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = (UnitOfWork)unitOfWork;
            _mapper = mapper;
        }
    }
}
