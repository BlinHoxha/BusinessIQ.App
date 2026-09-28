using BusinessIQ.Application.Businesses;
using BusinessIQ.Contracts.Businesses;
using BusinessIQ.Domain.Businesses;
using Framework.Web.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusinessIQ.Api.Controllers;

[Route("api/v1/businesses")]
[Authorize(Policy = "ManageBusinesses")]
public sealed class BusinessesController(ILogger<BusinessesController> logger, IBusinessService service)
    : BaseControllerCRUD<BusinessDto, BusinessGridDto, CreateBusinessDto, UpdateBusinessDto, Business, Guid, IBusinessService>(logger, service);

