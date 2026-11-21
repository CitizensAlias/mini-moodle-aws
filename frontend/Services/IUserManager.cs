namespace minimoodle.Services;

public interface IUserManager
{
    Task<Result<IEnumerable<MaterialDto>>> GetMaterials(CancellationToken cancellationToken = default);
    Task<Result<MaterialDto>> GetMaterial(string materialId, CancellationToken cancellationToken = default);
    Task<Result> ChangePassword(string newPassword, CancellationToken cancellationToken = default);
}
