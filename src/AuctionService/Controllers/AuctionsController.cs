using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext context;
    private readonly IMapper mapper;

    public AuctionsController(AuctionDbContext context, IMapper mapper)
    {
        this.context = context;
        this.mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDTO>>> GetAllAuctions()
    {
        var auctions = await this.context.Auctions
                .Include(x => x.Item)
                .OrderBy(x => x.Item.Make)
                .ToListAsync();

        return this.mapper.Map<List<AuctionDTO>>(auctions);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDTO>> GetAuctionById(Guid id)
    {
        var auction = await this.context.Auctions
                .Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);

        return this.mapper.Map<AuctionDTO>(auction);
    }

    [HttpPost]
    public async Task<ActionResult<AuctionDTO>> CreateAuction(CreateAuctionDto auctionDto)
    {
        //var auction = this.mapper.Map<Auction>(auctionDto);
        var auction = new Auction
        {
            Item = new Item
            {
                Make = auctionDto.Make,
                Model = auctionDto.Model,
                Year = auctionDto.Year,
                Color = auctionDto.Color,
                Mileage = auctionDto.Mileage,
                ImageUrl = auctionDto.ImageUrl
            },
            ReservePrice = auctionDto.ReservePrice,
            AuctionEnd = auctionDto.AuctionEnd
        };

        // TODO: add current user as seler
        auction.Seller = "test";

        this.context.Auctions.Add(auction);

        var result = await this.context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("Could not save changes to the DB");

        return CreatedAtAction(nameof(GetAuctionById),
                new { auction.Id }, 
                this.mapper.Map<AuctionDTO>(auction));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto updateAuction)
    {
        var auction = await this.context.Auctions.Include(x => x.Item)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (auction == null) return NotFound();

        // TODO: check seller == username
        auction.Item.Make = updateAuction.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuction.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuction.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuction.Mileage ?? auction.Item.Mileage;
        auction.Item.Year = updateAuction.Year ?? auction.Item.Year;

        var result = await this.context.SaveChangesAsync() > 0;

        if (result) return Ok();

        return BadRequest("Could not save changes");
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAuction(Guid id)
    {
        var auction = await this.context.Auctions.FindAsync(id);

        if (auction == null) return NotFound();

        this.context.Auctions.Remove(auction);

        var result = await this.context.SaveChangesAsync() > 0;

        if (!result) return BadRequest("could not delete from db");

        return Ok();
    }
}
