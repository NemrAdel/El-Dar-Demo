using AutoMapper;
using ElDarBLL.ViewModel.DataVM;
using ElDarBLL.ViewModel.EmployeeVM;
using ElDarBLL.ViewModel.GuestVM;
using ElDarDal.Models;

namespace ElDarBLL.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateGuestVM, Guest>();
            CreateMap<Guest, GuestVM>();

            CreateMap<UpdateGuestVM, Guest>();
                //.ForMember(dest => dest.Price, opt => opt.Condition(src => src.Price.HasValue))
                //.ForMember(dest => dest.OutPrice, opt => opt.Condition(src => src.OutPrice.HasValue))
                //.ForMember(dest => dest.Status, opt => opt.Condition(src => src.Status.HasValue))
                //.ForMember(dest => dest.DateOfBirth, opt => opt.Condition(src => src.DateOfBirth.HasValue))
                //.ForMember(dest => dest.PhoneNumber, opt => opt.Condition(src => !string.IsNullOrEmpty(src.PhoneNumber)))
                //.ForMember(dest => dest.Name, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Name)))
                //.ForMember(dest => dest.Address, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Address)))
                //.ForMember(dest => dest.Notes, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Notes)));


            CreateMap<CreateEmployeeVM, Employee>();
            CreateMap<Employee, EmployeeVM>();

            CreateMap<UpdateEmployeeVM, Employee>();
                //.ForMember(dest => dest.Salary, opt => opt.Condition(src => src.Salary.HasValue))
                //.ForMember(dest => dest.Taxs, opt => opt.Condition(src => src.Taxs.HasValue))
                //.ForMember(dest => dest.Adds, opt => opt.Condition(src => src.Adds.HasValue))
                //.ForMember(dest => dest.PhoneNumber, opt => opt.Condition(src => !string.IsNullOrEmpty(src.PhoneNumber)))
                //.ForMember(dest => dest.DateOfBirth, opt => opt.Condition(src => src.DateOfBirth.HasValue))
                //.ForMember(dest => dest.Job, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Job)))
                //.ForMember(dest => dest.Name, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Name)))
                //.ForMember(dest => dest.Address, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Address)))
                //.ForMember(dest => dest.Notes, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Notes)));
            
            
            CreateMap<CreateDataVM, Data>();
            CreateMap<Data, DataVM>();
            CreateMap<UpdateDataVM, Data>();
                //.ForMember(dest => dest.Price, opt => opt.Condition(src => src.Price.HasValue))
                //.ForMember(dest => dest.OutPrice, opt => opt.Condition(src => src.OutPrice.HasValue))
                //.ForMember(dest => dest.Payment, opt => opt.Condition(src => src.Payment.HasValue))
                //.ForMember(dest => dest.Date, opt => opt.Condition(src => src.Date.HasValue))
                //.ForMember(dest => dest.Name, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Name)));
                //.ForMember(dest => dest.Notes, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Notes)));
        }   

    }
}