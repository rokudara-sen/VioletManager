using Microsoft.AspNetCore.Mvc;
using VioletManager.Application.Contacts.Commands;
using VioletManager.Application.Contacts.Results;
using VioletManager.Application.Handlers.Contacts;

namespace VioletManager.API.Controllers;

[ApiController]
[Route(ContactsRoute)]
public sealed class ContactController : ControllerBase
{
    private const string ContactsRoute = "api/v1/contacts";

    private readonly CreateContactHandler _createContactHandler;
    private readonly DeleteContactHandler _deleteContactHandler;
    private readonly GetContactHandler _getContactHandler;

    public ContactController(CreateContactHandler createContactHandler, DeleteContactHandler deleteContactHandler,
        GetContactHandler getContactHandler)
    {
        _createContactHandler = createContactHandler;
        _deleteContactHandler = deleteContactHandler;
        _getContactHandler = getContactHandler;
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
            ContactUri(result.ContactId),
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

    [HttpGet("{contactId:guid}")]
    [ProducesResponseType(typeof(GetContactResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetContactResult>> GetAsync(Guid contactId, CancellationToken cancellationToken)
    {
        GetContactResult? result;

        try
        {
            result = await _getContactHandler.HandleAsync(
                new GetContactCommand(contactId),
                cancellationToken);
        }
        catch (ArgumentException exception)
        {
            return Problem(
                detail: exception.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (result is null)
        {
            return Problem(
                detail: $"No contact with ID '{contactId}' exists.",
                statusCode: StatusCodes.Status404NotFound);
        }

        Response.Headers.Location = ContactUri(contactId);

        return Ok(result);
    }

    private static string ContactUri(Guid contactId)
    {
        return $"/{ContactsRoute}/{contactId}";
    }
}
