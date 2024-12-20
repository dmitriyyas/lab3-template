using GatewayService.Dto;
using GatewayService.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace GatewayService.Controllers;

[ApiController]
[Route("")]
public class GatewayController(RequestQueue requestQueue,
    BonusService bonusService,
    FlightService flightService,
    TicketService ticketService) : ControllerBase
{
    private RequestQueue _requestQueue = requestQueue;
    private BonusService _bonusService = bonusService;
    private FlightService _flightService = flightService;
    private TicketService _ticketService = ticketService;

    private static bool IsSuccessCode(int code) => code < 300;
    private static bool IsClientError(int code) => 400 <= code && code < 500;
    private static bool IsServerError(int code) => code >= 500;

    [HttpGet("manage/health")]
    public IActionResult Health()
    {
        return Ok();
    }

    [HttpGet("api/v1/flights")]
    public async Task<IActionResult> GetFlights([FromQuery] int? page = null, [FromQuery] int? size = null)
    {
        var response = await _flightService.GetFlights(null, page, size);
        if (IsSuccessCode(response.StatusCode))
            return Ok(response.Response);
        return StatusCode(response.StatusCode, response.ErrorDto);
    }

    [HttpGet("api/v1/me")]
    public async Task<IActionResult> GetUserInfo([FromHeader(Name = "X-User-Name")] string username)
    {
        var ticketsResponse = await _ticketService.GetTickets(username);
        if (!IsSuccessCode(ticketsResponse.StatusCode))
            return StatusCode(ticketsResponse.StatusCode, ticketsResponse.ErrorDto);
        var tickets = ticketsResponse.Response!;

        var flightsResponse = await _flightService.GetFlights(tickets.Select(t => t.FlightNumber).ToList());
        var flights = IsSuccessCode(flightsResponse.StatusCode) ? flightsResponse.Response! : new(0, 1, 0, []);

        var ticketDetails = tickets.Select(ticket =>
        {
            var flight = flights.Items.First(f => f.FlightNumber == ticket.FlightNumber);
            return new TicketDetailDto(ticket.TicketUid,
                ticket.FlightNumber,
                flight.FromAirport,
                flight.ToAirport,
                flight.Date,
                ticket.Price,
                ticket.Status);
        }).ToList();

        var privilegeResponse = await _bonusService.GetPrivileges(username);
        var privilege = privilegeResponse.Response;

        object shortPrivilege = privilege is not null ? new ShortPrivilegeDto(privilege.Balance, privilege.Status) : "";
        var userInfo = new UserInfoDto(ticketDetails, shortPrivilege);

        return Ok(userInfo);
    }

    [HttpGet("api/v1/tickets")]
    public async Task<IActionResult> GetTickets([FromHeader(Name = "X-User-Name")] string username)
    {
        var ticketsResponse = await _ticketService.GetTickets(username);
        if (!IsSuccessCode(ticketsResponse.StatusCode))
            return StatusCode(ticketsResponse.StatusCode, ticketsResponse.ErrorDto);
        var tickets = ticketsResponse.Response!;

        var flightsResponse = await _flightService.GetFlights(tickets.Select(t => t.FlightNumber).ToList());
        var flights = IsSuccessCode(flightsResponse.StatusCode) ? flightsResponse.Response! : new(0, 1, 0, []);

        var ticketDetails = tickets.Select(ticket =>
        {
            var flight = flights.Items.First(f => f.FlightNumber == ticket.FlightNumber);
            return new TicketDetailDto(ticket.TicketUid,
                ticket.FlightNumber,
                flight.FromAirport,
                flight.ToAirport,
                flight.Date,
                ticket.Price,
                ticket.Status);
        }).ToList();

        return Ok(ticketDetails);
    }

    [HttpGet("api/v1/privilege")]
    public async Task<IActionResult> GetPrivilege([FromHeader(Name = "X-User-Name")] string username)
    {
        Console.WriteLine("got request get privilege");
        var privilegeResponse = await _bonusService.GetPrivileges(username);
        if (IsSuccessCode(privilegeResponse.StatusCode))
            return Ok(privilegeResponse.Response);
        return StatusCode(privilegeResponse.StatusCode, privilegeResponse.ErrorDto);
    }

    [HttpGet("api/v1/tickets/{ticketUid}")]
    public async Task<IActionResult> GetTicket([FromHeader(Name = "X-User-Name")] string username, [FromRoute] Guid ticketUid)
    {
        var ticketsResponse = await _ticketService.GetTicket(username, ticketUid);
        if (!IsSuccessCode(ticketsResponse.StatusCode))
            return StatusCode(ticketsResponse.StatusCode, ticketsResponse.ErrorDto);
        var ticket = ticketsResponse.Response!;

        var flightResponse = await _flightService.GetFlight(ticket.FlightNumber);
        if (IsClientError(flightResponse.StatusCode))
            return StatusCode(flightResponse.StatusCode, flightResponse.ErrorDto);
        var flight = flightResponse.Response;

        var ticketDetail = new TicketDetailDto(ticket.TicketUid,
            ticket.FlightNumber,
            flight?.FromAirport ?? "",
            flight?.ToAirport ?? "",
            flight?.Date ?? new DateTime(),
            ticket.Price,
            ticket.Status);

        return Ok(ticketDetail);
    }

    [HttpDelete("api/v1/tickets/{ticketUid}")]
    public async Task<IActionResult> CancelTicket([FromHeader(Name = "X-User-Name")] string username, [FromRoute] Guid ticketUid)
    {
        var ticketResponse = await _ticketService.CancelTicket(username, ticketUid);
        if (!IsSuccessCode(ticketResponse.StatusCode))
            return StatusCode(ticketResponse.StatusCode, ticketResponse.ErrorDto);

        var privilegeResponse = await _bonusService.CancelTicket(username, ticketUid);
        if (IsClientError(privilegeResponse.StatusCode))
            return StatusCode(privilegeResponse.StatusCode, privilegeResponse.ErrorDto);
        if (IsServerError(privilegeResponse.StatusCode))
        {
            _requestQueue.AddRequestToQueue(async () =>
            {
                var res = await _bonusService.CancelTicket(username, ticketUid);
                return !IsServerError(res.StatusCode);
            });
        }

        return NoContent();
    }

    [HttpPost("api/v1/tickets")]
    public async Task<IActionResult> BuyTicket([FromHeader(Name = "X-User-Name")] string username, PurchaseRequestDto purchaseRequest)
    {
        var flightResponse = await _flightService.GetFlight(purchaseRequest.FlightNumber);
        if (!IsSuccessCode(flightResponse.StatusCode))
            return StatusCode(flightResponse.StatusCode, flightResponse.ErrorDto);
        var flight = flightResponse.Response!;

        var ticketCreate = new TicketCreateDto(purchaseRequest.FlightNumber, purchaseRequest.Price);
        var ticketResponse = await _ticketService.CreateTicket(username, ticketCreate);
        if (!IsSuccessCode(ticketResponse.StatusCode))
            return StatusCode(ticketResponse.StatusCode, ticketResponse.ErrorDto);
        var ticket = ticketResponse.Response!;

        var ticketInfo = new TicketInfoDto(purchaseRequest.Price, purchaseRequest.PaidFromBalance, ticket.TicketUid, flight.Date);
        var buyResponse = await _bonusService.BuyTicket(username, ticketInfo);
        if (!IsSuccessCode(buyResponse.StatusCode))
        {
            await _ticketService.CancelTicket(username, ticket.TicketUid);
            return StatusCode(buyResponse.StatusCode, buyResponse.ErrorDto);
        }

        var purchaseInfo = buyResponse.Response!;

        var privilegeResponse = await _bonusService.GetPrivileges(username);
        var privilege = privilegeResponse.Response;

        object shortPrivilege = privilege is not null ? new ShortPrivilegeDto(privilege.Balance, privilege.Status) : "";

        var result = new PurchaseResponseDto(ticket.TicketUid,
            flight.FlightNumber,
            flight.FromAirport,
            flight.ToAirport,
            flight.Date,
            purchaseRequest.Price,
            purchaseInfo.PaidByMoney,
            purchaseInfo.PaidByBonuses,
            ticket.Status,
            shortPrivilege);

        return Ok(result);
    }
}
