using AutoMapper;
using Project.Application.Dtos.Employee;
using Project.Application.Dtos.LostProperties;
using Project.Core.Entities;

namespace Project.Application.AutoMapper
{
    public class ApplicationProfile : Profile
    {
        public ApplicationProfile()
        {
            CreateMap<EmployeeDto, Employee>().ReverseMap();
            CreateMap<CreateEmployeeDto, Employee>();
            CreateMap<UpdateEmployeeDto, Employee>();

            CreateMap<LostPropertyDto, LostProperty>().ReverseMap();
            CreateMap<CreateLostPropertyDto, LostProperty>();
            CreateMap<UpdateLostPropertyDto, LostProperty>();
        }
    }
}
