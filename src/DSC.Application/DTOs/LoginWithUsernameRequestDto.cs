namespace DSC.Application.DTOs;

public record class LoginWithUsernameRequestDto(string Username = "", string Password = "");
