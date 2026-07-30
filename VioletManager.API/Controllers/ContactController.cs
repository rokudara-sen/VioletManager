using Microsoft.AspNetCore.Mvc;
using VioletManager.Application;

namespace VioletManager.API.Controllers;

[ApiController]
[Route("api/v1/contacts")]
public sealed class ContactController : ControllerBase
{
    private readonly CreateContactHandler _createContactHandler;

    public ContactController(CreateContactHandler createContactHandler)
    {
        _createContactHandler = createContactHandler;
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateContactResult), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateContactResult>> CreateAsync(
        CreateContactCommand command,
        CancellationToken cancellationToken)
    {
        CreateContactResult result;

        try
        {
            result = await _createContactHandler.HandleAsync(
                command,
                cancellationToken);
        }
        catch (ArgumentException exception)
        {
            return Problem(
                detail: exception.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Created(
            $"/api/v1/contacts/{result.ContactId}",
            result);
    }
}
