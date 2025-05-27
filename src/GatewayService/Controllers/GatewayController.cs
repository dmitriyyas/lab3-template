using Common;
using GatewayService.Dto;
using GatewayService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace GatewayService.Controllers;

[ApiController]
[Route("")]
public class GatewayController(RequestQueue requestQueue,
    BonusService bonusService,
    FlightService flightService,
    TicketService ticketService,
    AuthService authService,
    StatsService statsService) : ControllerBase
{
    private RequestQueue _requestQueue = requestQueue;
    private BonusService _bonusService = bonusService;
    private FlightService _flightService = flightService;
    private TicketService _ticketService = ticketService;
    private AuthService _authService = authService;
    private StatsService _statsService = statsService;


    [HttpGet("manage/health")]
    public IActionResult Health()
    {
        return Ok();
    }

    [HttpPost("api/v1/login")]
    public async Task<IActionResult> Login([FromBody] AuthDataDto authData)
    {
        var response = await _authService.Login(authData);

        return StatusCode(response.StatusCode, response.Response);
    }

    [HttpPost("api/v1/users")]
    public async Task<IActionResult> Register([FromBody] UserCreateDto userCreate)
    {
        var response = await _authService.Register(userCreate);

        return StatusCode(response.StatusCode, response.Response);
    }

    [HttpPost("api/v1/token")]
    public async Task<IActionResult> GetToken([FromBody] TokenRequestDto tokenRequest)
    {
        var response = await _authService.GetToken(tokenRequest);

        return StatusCode(response.StatusCode, response.Response);
    }

    [HttpGet("api/v1/flights")]
    public async Task<IActionResult> GetFlights([FromQuery] int? page = null, [FromQuery] int? size = null)
    {
        var response = await _flightService.GetFlights(null, page, size);
        if (StatusCodeHelper.IsSuccessCode(response.StatusCode))
            return Ok(response.Response);
        return StatusCode(response.StatusCode, response.ErrorDto);
    }

    [HttpGet("api/v1/stats")]
    [Authorize]
    public async Task<IActionResult> GetStats([FromQuery] int? page = null, [FromQuery] int? size = null)
    {
        var accessToken = HttpContext.GetAccessToken();

        var response = await _statsService.GetStats(accessToken, page, size);
        if (StatusCodeHelper.IsSuccessCode(response.StatusCode))
            return Ok(response.Response);
        return StatusCode(response.StatusCode, response.ErrorDto);
    }

    [HttpGet("api/v1/me")]
    [Authorize]
    public async Task<IActionResult> GetUserInfo()
    {
        var accessToken = HttpContext.GetAccessToken();

        var userResponse = await _authService.GetUserInfo(accessToken);
        if (!StatusCodeHelper.IsSuccessCode(userResponse.StatusCode))
            return StatusCode(userResponse.StatusCode, userResponse.ErrorDto);
        var user = userResponse.Response!;

        var ticketsResponse = await _ticketService.GetTickets(accessToken);
        var tickets = StatusCodeHelper.IsSuccessCode(ticketsResponse.StatusCode) ? ticketsResponse.Response! : [];

        var flightsResponse = await _flightService.GetFlights(tickets.Select(t => t.FlightNumber).ToList());
        var flights = StatusCodeHelper.IsSuccessCode(flightsResponse.StatusCode) ? flightsResponse.Response! : new(0, 1, 0, []);

        var ticketDetails = tickets.Select(ticket =>
        {
            var flight = flights.Items.FirstOrDefault(f => f.FlightNumber == ticket.FlightNumber);
            return new TicketDetailDto(ticket.TicketUid,
                ticket.FlightNumber,
                flight?.FromAirport ?? "",
                flight?.ToAirport ?? "",
                flight?.Date ?? new DateTime(),
                ticket.Price,
                ticket.Status);
        }).ToList();

        var privilegeResponse = await _bonusService.GetPrivileges(accessToken);
        var privilege = privilegeResponse.Response;

        var shortPrivilege = privilege is not null ? new ShortPrivilegeDto(privilege.Balance, privilege.Status) : null;
        var userInfo = new UserInfoDto(user, ticketDetails, shortPrivilege);

        return Ok(userInfo);
    }

    [HttpGet("api/v1/tickets")]
    [Authorize]
    public async Task<IActionResult> GetTickets()
    {
        var accessToken = HttpContext.GetAccessToken();

        var ticketsResponse = await _ticketService.GetTickets(accessToken);
        if (!StatusCodeHelper.IsSuccessCode(ticketsResponse.StatusCode))
            return StatusCode(ticketsResponse.StatusCode, ticketsResponse.ErrorDto);
        var tickets = ticketsResponse.Response!;

        var flightsResponse = await _flightService.GetFlights(tickets.Select(t => t.FlightNumber).ToList());
        var flights = StatusCodeHelper.IsSuccessCode(flightsResponse.StatusCode) ? flightsResponse.Response! : new(0, 1, 0, []);

        var ticketDetails = tickets.Select(ticket =>
        {
            var flight = flights.Items.FirstOrDefault(f => f.FlightNumber == ticket.FlightNumber);
            return new TicketDetailDto(ticket.TicketUid,
                ticket.FlightNumber,
                flight?.FromAirport ?? "",
                flight?.ToAirport ?? "",
                flight?.Date ?? new DateTime(),
                ticket.Price,
                ticket.Status);
        }).ToList();

        return Ok(ticketDetails);
    }

    [HttpGet("api/v1/privilege")]
    [Authorize]
    public async Task<IActionResult> GetPrivilege()
    {
        var accessToken = HttpContext.GetAccessToken();

        var privilegeResponse = await _bonusService.GetPrivileges(accessToken);
        if (StatusCodeHelper.IsSuccessCode(privilegeResponse.StatusCode))
            return Ok(privilegeResponse.Response);
        return StatusCode(privilegeResponse.StatusCode, privilegeResponse.ErrorDto);
    }

    [HttpGet("api/v1/tickets/{ticketUid}")]
    [Authorize]
    public async Task<IActionResult> GetTicket([FromRoute] Guid ticketUid)
    {
        var accessToken = HttpContext.GetAccessToken();

        var ticketsResponse = await _ticketService.GetTicket(accessToken, ticketUid);
        if (!StatusCodeHelper.IsSuccessCode(ticketsResponse.StatusCode))
            return StatusCode(ticketsResponse.StatusCode, ticketsResponse.ErrorDto);
        var ticket = ticketsResponse.Response!;

        var flightResponse = await _flightService.GetFlight(ticket.FlightNumber);
        if (StatusCodeHelper.IsClientError(flightResponse.StatusCode))
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
    [Authorize]
    public async Task<IActionResult> CancelTicket([FromRoute] Guid ticketUid)
    {
        var accessToken = HttpContext.GetAccessToken();

        var ticketResponse = await _ticketService.CancelTicket(accessToken, ticketUid);
        if (!StatusCodeHelper.IsSuccessCode(ticketResponse.StatusCode))
            return StatusCode(ticketResponse.StatusCode, ticketResponse.ErrorDto);

        var privilegeResponse = await _bonusService.CancelTicket(accessToken, ticketUid);
        if (StatusCodeHelper.IsClientError(privilegeResponse.StatusCode))
            return StatusCode(privilegeResponse.StatusCode, privilegeResponse.ErrorDto);
        if (StatusCodeHelper.IsServerError(privilegeResponse.StatusCode))
        {
            _requestQueue.AddRequestToQueue(async () =>
            {
                var res = await _bonusService.CancelTicket(accessToken, ticketUid);
                return !StatusCodeHelper.IsServerError(res.StatusCode);
            });
        }

        return NoContent();
    }

    [HttpPost("api/v1/tickets")]
    [Authorize]
    public async Task<IActionResult> BuyTicket(PurchaseRequestDto purchaseRequest)
    {
        var accessToken = HttpContext.GetAccessToken();

        var flightResponse = await _flightService.GetFlight(purchaseRequest.FlightNumber);
        if (!StatusCodeHelper.IsSuccessCode(flightResponse.StatusCode))
            return StatusCode(flightResponse.StatusCode, flightResponse.ErrorDto);
        var flight = flightResponse.Response!;

        var ticketCreate = new TicketCreateDto(purchaseRequest.FlightNumber, purchaseRequest.Price);
        var ticketResponse = await _ticketService.CreateTicket(accessToken, ticketCreate);
        if (!StatusCodeHelper.IsSuccessCode(ticketResponse.StatusCode))
            return StatusCode(ticketResponse.StatusCode, ticketResponse.ErrorDto);
        var ticket = ticketResponse.Response!;

        var ticketInfo = new TicketInfoDto(purchaseRequest.Price, purchaseRequest.PaidFromBalance, ticket.TicketUid, flight.Date);
        var buyResponse = await _bonusService.BuyTicket(accessToken, ticketInfo);
        if (!StatusCodeHelper.IsSuccessCode(buyResponse.StatusCode))
        {
            await _ticketService.CancelTicket(accessToken, ticket.TicketUid);
            return StatusCode(buyResponse.StatusCode, buyResponse.ErrorDto);
        }

        var purchaseInfo = buyResponse.Response!;

        var privilegeResponse = await _bonusService.GetPrivileges(accessToken);
        var privilege = privilegeResponse.Response;

        var shortPrivilege = privilege is not null ? new ShortPrivilegeDto(privilege.Balance, privilege.Status) : null;

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
