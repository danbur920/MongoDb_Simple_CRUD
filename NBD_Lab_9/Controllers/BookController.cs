using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using NBD_Lab_9.Models;

namespace NBD_Lab_9.Controllers
{
    public class BookController : Controller
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoDatabase _database;
        private readonly IMongoCollection<Book> _books;

        public BookController(IConfiguration configuration, IMongoClient mongoClient)
        {
            var databaseSettings = configuration.GetSection("MongoDbSettings");
            var databaseName = databaseSettings["DatabaseName"];

            _mongoClient = mongoClient;
            _database = _mongoClient.GetDatabase(databaseName);
            _books = _database.GetCollection<Book>("Books");
        }
        public async Task<IActionResult> Index()
        {
            var collection = _database.GetCollection<Book>("Books");
            var books = await collection.Find(x => true).ToListAsync();
            return View(books);
        }

        [HttpGet]
        public async Task<IActionResult> Add()
        {
            var book = new Book();
            return View(book);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Book book)
        {
            if (ModelState.IsValid)
            {
                await _books.InsertOneAsync(book);
                return RedirectToAction("Index", "Book");
            }
            return View(book);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var bookToDelete = await _books.FindAsync(x => x.Id == id);
            if (bookToDelete != null)
                await _books.DeleteOneAsync(a => a.Id == id);

            return RedirectToAction("Index");
        }
  
        [HttpGet]
        public async Task<IActionResult> Update(string id)
        {
            var book = await _books.Find(x => x.Id == id).FirstOrDefaultAsync();
            return View(book);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Book book)
        {
            if (ModelState.IsValid)
            {
                var result = await _books.ReplaceOneAsync(
                    x => x.Id == book.Id,
                    book
                );

                if (result.ModifiedCount > 0)
                    return RedirectToAction("Index", "Book");
            }
            return View(book);
        }
    }
}
