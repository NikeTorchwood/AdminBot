namespace AdminBot.Entities;

public class ChangeRoleRequest
{
    public long ApplicantId { get; }
    public Roles NewRole { get; }
    public string? StoreCode { get; }

    public ChangeRoleRequest(long applicantId, Roles newRole, string? storeCode)
    {
        ApplicantId = applicantId;
        NewRole = newRole;
        StoreCode = storeCode;
    }
}