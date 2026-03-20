namespace BuildingBlocks.Identity;

public static class Roles
{
    public const string Admin = "Admin";
    public const string SuperAdmin = "SuperAdmin";
    public const string Attemper = "Attemper";
    public const string ProblemSetter = "ProblemSetter";
    public const string User = "User";
        
    public static readonly string[] AllRoles = { Admin, SuperAdmin, Attemper, ProblemSetter, User };
}