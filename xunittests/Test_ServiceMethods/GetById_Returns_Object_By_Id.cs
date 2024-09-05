//
// using dataaccess;
// using dataaccess.Models;
// using Microsoft.Extensions.DependencyInjection;
//
// using PgCtxSetup;
// using Xunit.Abstractions;
//
//
// namespace UnitTests;
//
//     public class GetDoctorById_GetsExistingDoctor_ReturnsDoctorDtoTest 
//     {
//         private readonly PgCtxSetup<HospitalContext> _pgCtxSetup;
//         
//         public GetDoctorById_GetsExistingDoctor_ReturnsDoctorDtoTest(
//             ITestOutputHelper outputHelper)
//
//         {
//             _pgCtxSetup = new PgCtxSetup<HospitalContext>(configureServices: services =>
//             {
//                 services.AddTransient<DoctorService>(serviceProvider =>
//                 {
//                     var context = serviceProvider.GetRequiredService<HospitalContext>();
//                     return new DoctorService(context);
//                 });
//             });
//    
//         }
//
//         [Fact]
//         public void GetDoctorById_ThrowsException_WhenNoDoctorExists()
//         {
//             //arrange
//             var doctor = new Doctor() { Name = "Bob", Specialty = "General", YearsExperience = 3, Id = 1 };
//             _pgCtxSetup.DbContextInstance.Doctors.Add(doctor);
//             
//             //act
//             var doctorDto = _pgCtxSetup.ServiceProviderInstance.GetRequiredService<DoctorService>().GetDoctorById(1);
//             
//             //assert
//             Assert.Equal(doctor.Name, doctorDto.Name);
//             Assert.Equal(doctor.Specialty, doctorDto.Specialty);
//             Assert.Equal(doctor.YearsExperience, doctorDto.YearsExperience);
//             Assert.Equal(doctor.Id, doctorDto.Id);
//         }
//         
//      
//
//     }
//
//     