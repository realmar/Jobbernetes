using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Realmar.Jobbernetes.Demo.Models;

namespace Realmar.Jobbernetes.Demo.DataViewer.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly IMongoCollection<Image> _collection;

        public ImagesController(IMongoCollection<Image> collection) => _collection = collection;

        [HttpGet]
        public IEnumerable<Image> Get() => _collection.AsQueryable();
    }
}
