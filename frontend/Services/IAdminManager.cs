namespace minimoodle.Services;

public interface IAdminManager
{
    Task<Result<IEnumerable<UserDto>>> GetUsers(CancellationToken cancellationToken = default);
    Task<Result<UserDto>> CreateUser(string userId, string temporaryPassword, CancellationToken cancellationToken = default);
    Task<Result> DeleteUser(string userId, CancellationToken cancellationToken = default);
    Task<Result> ChangePassword(string userId, string newPassword, CancellationToken cancellationToken = default);
    Task<Result<MaterialDto>> CreateMaterial(string materialId, MaterialType materialType, string content, CancellationToken cancellationToken = default);
    Task<Result<MaterialDto>> UpdateMaterial(string materialId, MaterialType materialType, string content, CancellationToken cancellationToken = default);
    Task<Result> DeleteMaterial(string materialId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<string>>> GetMaterialAccess(string materialId, CancellationToken cancellationToken = default);
    Task<Result> SetMaterialAccess(string materialId, IEnumerable<string> userIds, CancellationToken cancellationToken = default);
}
