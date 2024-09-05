using System.ComponentModel.DataAnnotations;
using dataaccess;
using Microsoft.Extensions.Logging;
using service.responses;

public class DoctorService(HospitalContext context, ILogger<DoctorService> logger, DoctorValidator validator)
{
    public DoctorDto GetDoctorById(int id)
    {
        var doctor = context.Doctors.Find(id);

        if (doctor == null)
        {
            throw new KeyNotFoundException("Doctor not found");
        }

        return new DoctorDto().FromEntity(doctor);

    }
}

public class DoctorValidator
{
    public void ValidateDoctor(DoctorDto dto)
    {
        if (dto.Name.Length < 3) throw new ValidationException();
    }
}