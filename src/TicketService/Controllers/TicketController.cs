using Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketService.Dto;
using TicketService.Models;

namespace TicketService.Controllers;

public class TicketController(TicketsContext context) : ControllerBase
{
    private readonly TicketsContext _context = context;

    [HttpGet("manage/health")]
    public IActionResult Health()
    {
        return Ok();
    }

    [HttpGet("api/v1/tickets")]
    [Authorize]
    public async Task<IActionResult> GetTickets()
    {
        var tickets = await _context.Tickets.Where(t => t.Username == HttpContext.GetUsername()).ToListAsync();

        return Ok(tickets.Select(t => ToDto(t)).ToList());
    }

    [HttpGet("api/v1/tickets/{ticketUid}")]
    [Authorize]
    public async Task<IActionResult> GetTicket([FromRoute] Guid ticketUid)
    {
        var ticket = await _context.Tickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TicketUid == ticketUid);

        if (ticket is null)
            return NotFound($"Ticket with uuid = {ticketUid} wasn't found");

        var username = HttpContext.GetUsername();
        if (ticket.Username != username)
            return StatusCode(StatusCodes.Status403Forbidden, $"Ticket with uuid = {ticketUid} doesn't belong to user with name = {username}");

        return Ok(ToDto(ticket));
    }

    [HttpDelete("api/v1/tickets/{ticketUid}")]
    [Authorize]
    public async Task<IActionResult> CancelTicket([FromRoute] Guid ticketUid)
    {
        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.TicketUid == ticketUid);

        if (ticket is null)
            return NotFound($"Ticket with uuid = {ticketUid} wasn't found");

        var username = HttpContext.GetUsername();
        if (ticket.Username != username)
            return StatusCode(StatusCodes.Status403Forbidden, $"Ticket with uuid = {ticketUid} doesn't belong to user with name = {username}");

        ticket.Status = "CANCELED";
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("api/v1/tickets")]
    [Authorize]
    public async Task<IActionResult> BuyTicket([FromBody] TicketCreateDto ticketCreate)
    {
        var username = HttpContext.GetUsername();
        var ticket = new Ticket
        {
            TicketUid = Guid.NewGuid(),
            Username = username,
            FlightNumber = ticketCreate.FlightNumber,
            Price = ticketCreate.Price,
            Status = "PAID"
        };

        var result = await _context.Tickets.AddAsync(ticket);
        await _context.SaveChangesAsync();

        return Ok(ToDto(result.Entity));
    }

    private static TicketDto ToDto(Ticket ticket)
    {
        return new TicketDto(ticket.TicketUid,
            ticket.FlightNumber,
            ticket.Price,
            ticket.Status);
    }

}
