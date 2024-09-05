using dataaccess;
using dataaccess.Models;

namespace DataAccess;

public class EasyRepository(HospitalContext context)
{
    public List<Doctor> GetAllDoctors()
    {
        return context.Doctors.ToList();
    }
}