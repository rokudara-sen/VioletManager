using Microsoft.AspNetCore.Mvc;
using VioletManager.Application;
using VioletManager.Application.Contacts.Commands;
using VioletManager.Application.Contacts.Results;
using VioletManager.Application.Handlers;
using VioletManager.Application.Handlers.Contacts;

namespace VioletManager.API.Controllers;

[ApiController]
[Route("api/v1/contacts")]
public sealed class ContactController : ControllerBase
{
    private readonly CreateContactHandler _createContactHandler;
    private readonly DeleteContactHandler _deleteContactHandler;

    public ContactController(CreateContactHandler createContactHandler,  DeleteContactHandler deleteContactHandler)
    {
        _createContactHandler = createContactHandler;
        _deleteContactHandler = deleteContactHandler;
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
            result = await _createContactHandler.HandleAsync(command, cancellationToken);
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

    [HttpDelete("{contactId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAsync(Guid contactId, CancellationToken cancellationToken)
    {
        bool deleted;

        try
        {
            deleted = await _deleteContactHandler.HandleAsync(
                new DeleteContactCommand(contactId),
                cancellationToken);
        }
        catch (ArgumentException exception)
        {
            return Problem(
                detail: exception.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (!deleted)
        {
            return Problem(
                detail: $"No contact with ID '{contactId}' exists.",
                statusCode: StatusCodes.Status404NotFound);
        }

        return NoContent();
    }
}
