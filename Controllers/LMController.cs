using LMS_API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;

namespace LMS_API.Controllers
{
    [EnableCors(origins: "http://localhost:4200", headers: "*", methods: "*")]
    public class LMController : ApiController
    {

        [RoutePrefix("api/library")]
        public class LibraryController : ApiController
        {
            private LibraryDBEntities db = new LibraryDBEntities();

            [HttpOptions]
            public IHttpActionResult Options()
            {
                return Ok();
            }

            // Get all books
            [HttpGet]
            [Route("books")]
            public IHttpActionResult GetBooks()
            {
                var books = db.Books.ToList();
                return Ok(books);
            }

            // Borrow a book
            [HttpPost]
            [Route("borrow")]
            public IHttpActionResult BorrowBook(int studentId, int bookId)
            {
                var book = db.Books.Find(bookId);
                if (book == null || book.Quantity <= 0)
                    return BadRequest("Book not available");

                var borrow = new BorrowHistory
                {
                    StudentId = studentId,
                    BookId = bookId,
                    BorrowDate = DateTime.Now,
                    Status = "Borrowed"
                };
                db.BorrowHistories.Add(borrow);
                book.Quantity -= 1;
                db.SaveChanges();
                return Ok("Book borrowed successfully");
            }

            // Return a book
            //[HttpPost]
            //[Route("return")]
            //public IHttpActionResult ReturnBook(int borrowId)
            //{
            //    var borrow = db.BorrowHistories.Find(borrowId);
            //    if (borrow == null || borrow.Status == "Returned")
            //        return BadRequest("Invalid borrow record");

            //    borrow.ReturnDate = DateTime.Now;
            //    borrow.Status = "Returned";
            //    var book = db.Books.Find(borrow.BookId);
            //    if (book != null)
            //    {
            //        book.Quantity += 1;
            //    }
            //    db.SaveChanges();
            //    return Ok("Book returned successfully");
            //}

            // Get users
            [HttpGet]
            [Route("users")]
            public IHttpActionResult GetUsers()
            {
                var users = db.Users.Select(u => new { u.Id, u.Username, u.Role }).ToList();
                return Ok(users);
            }

            [HttpPost]
            [Route("login")]
            public IHttpActionResult Login(string username, string password)
            {
                string hashedPassword = HashPassword(password);
                var user = db.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == hashedPassword);
                if (user == null)
                    return Unauthorized();
                return Ok(new { message = "Login successful", role = user.Role, user });
            }

            // User Registration
            [HttpPost]
            [Route("register")]
            public IHttpActionResult Register(string username, string password, string role)
            {
                if (db.Users.Any(u => u.Username == username))
                    return BadRequest("Username already exists");

                var newUser = new User
                {
                    Username = username,
                    PasswordHash = HashPassword(password),
                    Role = role
                };
                db.Users.Add(newUser);
                db.SaveChanges();
                return Ok("User registered successfully");
            }

            // Admin: Add/Edit Users
            [HttpPost]
            [Route("users/add")]
            public IHttpActionResult AddUser(string username, string password, string role)
            {
                return Register(username, password, role);
            }

            [HttpPut]
            [Route("users/edit/{id}")]
            public IHttpActionResult EditUser(int id, string username, string password, string role)
            {
                var user = db.Users.Find(id);
                if (user == null)
                    return NotFound();

                user.Username = username;
                user.PasswordHash = HashPassword(password);
                user.Role = role;
                db.SaveChanges();
                return Ok("User updated successfully");
            }

            // Book Operations
            [HttpPost]
            [Route("books/add")]
            public IHttpActionResult AddBook(string title, string author, string isbn, int quantity)
            {
                var newBook = new Book { Title = title, Author = author, ISBN = isbn, Quantity = quantity };
                db.Books.Add(newBook);
                db.SaveChanges();
                return Ok("Book added successfully");
            }

            [HttpPut]
            [Route("books/edit/{id}")]
            public IHttpActionResult EditBook(int id, string title, string author, string isbn, int quantity)
            {
                var book = db.Books.Find(id);
                if (book == null)
                    return NotFound();

                book.Title = title;
                book.Author = author;
                book.ISBN = isbn;
                book.Quantity = quantity;
                db.SaveChanges();
                return Ok("Book updated successfully");
            }

            [HttpDelete]
            [Route("books/delete/{id}")]
            public IHttpActionResult DeleteBook(int id)
            {
                var book = db.Books.Find(id);
                if (book == null)
                    return NotFound();

                db.Books.Remove(book);
                db.SaveChanges();
                return Ok("Book deleted successfully");
            }

            [HttpPost]
            [Route("return/{borrowId}")]
            public IHttpActionResult ReturnBook(int borrowId)
            {
                var borrow = db.BorrowHistories.Find(borrowId);
                if (borrow == null || borrow.Status == "Returned")
                    return BadRequest("Invalid borrow record");

                borrow.ReturnDate = DateTime.Now;
                borrow.Status = "Returned";
                var book = db.Books.Find(borrow.BookId);
                if (book != null)
                {
                    book.Quantity += 1;
                }
                db.SaveChanges();
                return Ok("Book returned successfully");
            }

            private string HashPassword(string password)
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    StringBuilder builder = new StringBuilder();
                    foreach (byte b in bytes)
                        builder.Append(b.ToString("x2"));
                    return builder.ToString();
                }
            }
        }

    }
}
