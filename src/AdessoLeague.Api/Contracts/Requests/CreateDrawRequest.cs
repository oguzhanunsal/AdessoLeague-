using System.ComponentModel.DataAnnotations;

namespace AdessoLeague.Api.Contracts.Requests;

public sealed record CreateDrawRequest(int GroupCount, [Required] DrawnByRequest DrawnBy);

// No data annotations on the fields: the draw's validator owns those rules, so every rejection
// comes back in one shape. Adding [Required] here would short-circuit it with a different body.
public sealed record DrawnByRequest(string FirstName, string LastName);
