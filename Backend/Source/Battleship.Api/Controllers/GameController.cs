using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Battleship.Api.Models;
using Battleship.Business.Models.Contracts;
using Battleship.Business.Services.Contracts;
using Battleship.Domain;
using Battleship.Domain.FleetDomain;
using Battleship.Domain.GameDomain;
using Battleship.Domain.GridDomain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Battleship.Api.Controllers
{
    //FOR THE MINIMAL REQUIREMENTS IT IS NOT NEEDED TO TOUCH THIS FILE!!
    [Route("api/games")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;
        private readonly UserManager<User> _userManager;

        public GameController(IGameService gameService, UserManager<User> userManager)
        {
            _gameService = gameService;
            _userManager = userManager;
        }

        /// <summary>
        /// Starts a new game for the authenticated user.
        /// </summary>
        /// <param name="settings">Settings for the game. When no settings are posted, the default game settings will apply</param>
        [HttpPost("")]
        [ProducesResponseType(typeof(IGameInfo), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> CreateNewSinglePlayerGame(GameSettings settings = null)
        {
            if (settings == null)
            {
                settings = new GameSettings();
            }
            var currentUser = await _userManager.GetUserAsync(User);
            try
            {
                var game = _gameService.CreateGameForUser(settings, currentUser);
                return CreatedAtAction(nameof(GetGameInfo), new { id = game.Id }, game);
            }
            catch (ApplicationException applicationException)
            {
                ModelState.AddModelError("applicationException", applicationException.Message);
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// Starts a game. Should be called when both players have their fleet in position.
        /// After starting the game, te players can start shooting.
        /// </summary>
        /// <param name="id">The identifier of the game</param>
        [HttpPost("{id}/start")]
        [ProducesResponseType(typeof(Result), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> StartGame(Guid id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                Result result = _gameService.StartGame(id, currentUser.Id);
                return Ok(result);
            }
            catch (DataNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Returns info about the game with a matching id.
        /// The game info will be in the perspective of the authenticated user.
        /// </summary>
        /// <param name="id">The identifier of the game</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(IGameInfo), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> GetGameInfo(Guid id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                IGameInfo gameInfo = _gameService.GetGameInfoForPlayer(id, currentUser.Id);
                return Ok(gameInfo);
            }
            catch (DataNotFoundException)
            {
                return NotFound();
            }
        }

        /// <summary>
        /// Positions a ship on the grid of the authenticated user.
        /// </summary>
        /// <param name="id">The identifier of the game.</param>
        /// <param name="model">Contains info on which ship to position and where to position it.</param>
        [HttpPost("{id}/positionship")]
        [ProducesResponseType((typeof(Result)), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> PositionShipOnGrid(Guid id, ShipPositioningModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            try
            {
                GridCoordinate[] segmentCoordinates = model.SegmentCoordinates.Select(cm => new GridCoordinate(cm.Row, cm.Column)).ToArray();
                ShipKind shipKind = ShipKind.CreateFromCode(model.ShipCode);
                Result result = _gameService.PositionShipOnGrid(id, currentUser.Id, shipKind, segmentCoordinates);
                return Ok(result);
            }
            catch (DataNotFoundException)
            {
                ModelState.AddModelError("gameNotFound", $"Could not find a game with id {id}");
            }
            catch (ApplicationException applicationException)
            {
                ModelState.AddModelError("applicationException", applicationException.Message);
            }
            return BadRequest(ModelState);
        }

        /// <summary>
        /// Fires a bomb for the authenticated user on the opponents grid.
        /// </summary>
        /// <param name="id">The identifier of the game</param>
        /// <param name="model">The square coordinate of the opponents grid to hit.</param>
        [HttpPost("{id}/shoot")]
        [ProducesResponseType((typeof(ShotResult)), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.Forbidden)]
        public async Task<IActionResult> ShootAtOpponent(Guid id, GridCoordinateModel model)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            try
            {
                ShotResult result = _gameService.ShootAtOpponent(id, currentUser.Id, new GridCoordinate(model.Row, model.Column));
                return Ok(result);
            }
            catch (DataNotFoundException)
            {
                ModelState.AddModelError("gameNotFound", $"Could not find a game with id {id}");
            }
            catch (ApplicationException applicationException)
            {
                ModelState.AddModelError("applicationException", applicationException.Message);
            }
            return BadRequest(ModelState);
        }
    }
}