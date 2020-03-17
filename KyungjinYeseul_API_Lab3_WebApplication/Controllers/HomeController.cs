using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using KyungjinYeseul_API_Lab3_WebApplication.Models;
using Microsoft.AspNetCore.Http;
using System.Net.Mail;
using System.Text;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Data;
using Amazon.S3;
using System.IO;
using Amazon.S3.Transfer;
using Amazon;
using Amazon.S3.Model;

namespace KyungjinYeseul_API_Lab3_WebApplication.Controllers
{
    public class HomeController : Controller
    {
        Appointment_DBOperation dynamoDB = new Appointment_DBOperation("Appointment");

        private IUserRepository usersRepository;
        public IConfiguration Configuration { get; }
        int final;
        public HomeController(IUserRepository userRepository, IConfiguration configuration)
        {
            usersRepository = userRepository;
            Configuration = configuration;

            //test.CreateTable();
            //test.InsertAsync().Wait();
        }

        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignIn(User user)
        {
            if (user.Type == null) {
                TempData["validation"] = "Please select user type !";
            }
            else if (user.EmailID == null) {
                TempData["validation"] = "Please enter your email address !";
            }
            else if (user.Password == null) {
                TempData["validation"] = "Please enter your password !";
            }
            else {
                // check from RDS database!
                if(usersRepository.Users.Where(u => u.Type == user.Type & u.EmailID == user.EmailID & u.Password == user.Password).FirstOrDefault() != null)
                {

                    User loginUser = usersRepository.Users.Where(u => u.Type == user.Type & u.EmailID == user.EmailID & u.Password == user.Password).First();

                    if (user != null)
                    {
                        // login success
                        ViewBag.userID = loginUser.UserID;
                        ViewBag.emailID = loginUser.EmailID;
                        ViewBag.type = loginUser.Type;

                        if (user.Type == "Student")
                        {
                            ViewBag.studentFullName = loginUser.FirstName + " " + loginUser.LastName;

                            // select teacher List
                            IEnumerable<User> teacherList = usersRepository.Users.Where(u => u.Type == "Teacher");
                            return View("AppointmentBoard", teacherList);
                        }
                        else if (user.Type == "Teacher")
                        {
                            List<Appointment> appointmentList = dynamoDB.GetAppointmentsAsync(loginUser.EmailID, "Teacher").Result;

                            return View("AppointmentList", appointmentList);
                        }
                    }

                }
                else
                {
                    // login fail
                    TempData["validation"] = "Please check your login information !";
                }

            }
            return View();
        }

        public IActionResult EditInfo(int userID)
        {
            List<String> items = new List<String>();
            items = usersRepository.userTimeSlots(userID).Select(t => t.StrTimeSlot).ToList();

            User user = usersRepository.Users.Where(u => u.UserID == userID).First();

            ViewBag.items = items;

            ViewBag.userId = user.UserID;
            ViewBag.type = user.Type;
            ViewBag.emailId = user.EmailID;

            

            return View(usersRepository.Users.FirstOrDefault(u => u.UserID == userID));
        }

        [HttpPost]
        public async Task<IActionResult> EditInfo(User user, string[] time, IFormFile file)
        {
            if (user.LastName != null && user.FirstName != null && user.Type != null && user.EmailID != null && user.Password != null)
            {

                string connectionString = Configuration["ConnectionStrings:Connection2RDS"];
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string sql = $"Update [User] SET emailID='{user.EmailID}',password='{user.Password}',type='{user.Type}',firstName='{user.FirstName}',lastName='{user.LastName}' where userID='{user.UserID}'";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.CommandType = CommandType.Text;
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }
                }


                if (user.Type == "Teacher")
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {

                        User resultUser = usersRepository.Users.Where(u => u.EmailID == user.EmailID).First();
                        int userid = resultUser.UserID;

                        string sqlforTime = "";
                        string sqlforDelete = "";
                        sqlforDelete = $"Delete From [TimeSlot] Where userID = '{userid}'";

                        using (SqlCommand command = new SqlCommand(sqlforDelete, connection))
                        {
                            command.CommandType = CommandType.Text;
                            connection.Open();
                            command.ExecuteNonQuery();
                            connection.Close();
                        }

                        for (int i = 0; i < time.Length; i++)
                        {
                            List<string> item = new List<string>();

                            // item = usersRepository.userTimeSlots(userid).Select(t => t.UserID == userid & t.StrTimeSlot == time[i]).First();

                            //string sqlforID1 = $"SELECT timeSlotID from [TimeSlot] WHERE userID = " + userid + " AND timeSlot= '" + time[i].ToString() + "'";
                            //TimeSlot sqlforID2 = usersRepository.userTimeSlots(userid).Where(t => t.UserID == userid).First();
                            TimeSlot sqlforID3 = usersRepository.userTimeSlots(userid).Where(t => t.StrTimeSlot == time[i]).FirstOrDefault();
                            //TimeSlot sqlforID4 = usersRepository.userTimeSlots(userid).Where(t => t.UserID == userid & t.StrTimeSlot == time[i]).First();

                            //string sqlforTime = $"Insert Into [TimeSlot] (userID,timeSlot) Values({userid},'{time[i]}')";


                            sqlforTime = $"Insert Into [TimeSlot] (userID,timeSlot) Values({userid},'{time[i]}')";

                            using (SqlCommand command = new SqlCommand(sqlforTime, connection))
                            {
                                command.CommandType = CommandType.Text;
                                connection.Open();
                                command.ExecuteNonQuery();
                                connection.Close();
                            }
                        }
                    }
                }

                if (file != null)
                {
                    using (var client = new AmazonS3Client("AKIAVC2QJKV2O3OA32J5", "uFsz+kRG9CzEcIe6LhIzLp/o6pqdLLpt6guLUtKR", RegionEndpoint.CACentral1))
                    {
                        var bucketName = "lab3forfacultyresume";
                        var keyValue = string.Format(@"{0}/", user.EmailID);


                        ListObjectsRequest findFolder = new ListObjectsRequest();
                        findFolder.BucketName = bucketName;
                        findFolder.Prefix = keyValue;
                        ListObjectsResponse findFolderResponse = await client.ListObjectsAsync(findFolder);
                        Boolean folderExists = findFolderResponse.S3Objects.Any();

                        if (folderExists == true)
                        {
                            foreach (var entry in findFolderResponse.S3Objects)
                            {
                                await client.DeleteObjectAsync(bucketName, entry.Key);
                            }
                        }

                        PutObjectRequest request = new PutObjectRequest();
                        request.BucketName = "lab3forfacultyresume";
                        request.Key = keyValue;
                        request.InputStream = new MemoryStream();
                        PutObjectResponse response = await client.PutObjectAsync(request);


                        using (var newMemoryStream = new MemoryStream())
                        {
                            file.CopyTo(newMemoryStream);

                            var uploadRequest = new TransferUtilityUploadRequest
                            {
                                InputStream = newMemoryStream,
                                Key = keyValue + file.FileName,
                                BucketName = "lab3forfacultyresume",
                                CannedACL = S3CannedACL.PublicRead
                            };

                            var fileTransferUtility = new TransferUtility(client);
                            await fileTransferUtility.UploadAsync(uploadRequest);
                        }
                    }
                }

            }

            ViewBag.userId = user.UserID;
            ViewBag.type = user.Type;
            ViewBag.emailId = user.EmailID;

            return RedirectToAction("EditInfo", usersRepository.Users.FirstOrDefault(u => u.UserID == user.UserID));
        }


        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SignUp(User user, string[] time)
        {

            if (user.LastName != null && user.FirstName != null && user.Type != null && user.EmailID != null && user.Password != null)
            {
                string connectionString = Configuration["ConnectionStrings:Connection2RDS"];
                using (SqlConnection connection = new SqlConnection(connectionString))
                {

                    string sql = $"Insert Into [User] (emailID, password, type, firstName, lastName) Values ('{user.EmailID}', '{user.Password}','{user.Type}','{user.FirstName}','{user.LastName}')";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.CommandType = CommandType.Text;
                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }


                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {

                    User resultUser = usersRepository.Users.Where(u => u.EmailID == user.EmailID).First();
                    int userid = resultUser.UserID;

                    for (int i = 0; i < time.Length; i++)
                    {

                        string sqlforTime = $"Insert Into [TimeSlot] (userID,timeSlot) Values({userid},'{time[i]}')";
                        using (SqlCommand command = new SqlCommand(sqlforTime, connection))
                        {
                            command.CommandType = CommandType.Text;
                            connection.Open();
                            command.ExecuteNonQuery();
                            connection.Close();
                        }
                    }
                }
                return View("SignIn");
            }
            else
            {
                return View();
            }

        }

        [HttpGet]
        public IActionResult Appointment(string teacherEmailID, string appointmentDate_timeSlot, int userID, string type)
        {
            Appointment appointment = dynamoDB.getAppointmentAsync(teacherEmailID, appointmentDate_timeSlot).Result;

            ViewBag.userID = userID;
            ViewBag.flag = "Edit";

            ViewBag.type = type;
            if(type == "Student")
                ViewBag.emailID = appointment.StudentEmailID;
            else
                ViewBag.emailID = appointment.TeacherEmailID;

            return View(appointment);
        }

        [HttpPost]
        public IActionResult Appointment(Appointment appointment, int userID, string type, string emailID)
        {
            appointment.Status = "made";

            ViewBag.userID = userID;
            ViewBag.type = type;
            ViewBag.emailID = emailID;


            return View(appointment);
        }

        public IActionResult ReturnToAppointmentBoard(int userID, string type)
        {
            User loginUser = usersRepository.Users.Where(u => u.UserID == userID).First();

            ViewBag.userID = loginUser.UserID;
            ViewBag.emailID = loginUser.EmailID;
            ViewBag.studentFullName = loginUser.FirstName + " " + loginUser.LastName;
            ViewBag.type = type;

            return View("AppointmentBoard", usersRepository.Users.Where(u => u.Type == "Teacher"));
        }

        [HttpPost]
        public IActionResult SaveAppointment(Appointment appointment, int userID, string type)
        {
            Appointment saveAppointment = appointment;

            // send email
            saveAppointment.EmailMessage = sendEmail(appointment);

            // save appointment
            dynamoDB.InsertAppointmentAsync(saveAppointment);

            ViewBag.type = type;

            string emailID;
            if (type == "Teacher")
            {
                emailID = appointment.TeacherEmailID;
            }
            else
            {
                emailID = appointment.StudentEmailID;
            }


            ViewBag.emailID = emailID;

            // send to appointment list
            return RedirectToAction("AppointmentList", new { emailID, userID, ViewBag.type });
        }

        public string sendEmail(Appointment appointment)
        {
            string subject = "Appointment " + appointment.Status + " successfully!";
            string body = appointment.StudentFullName + ",<br/><br/>"
                + "You have successfully " + appointment.Status + " an appointment for " + appointment.Course
                + " on " + appointment.AppointmentDate_TimeSlot.Replace("_", " during ") + ".";

            if(appointment.Status != "canceled")
            {
                body += "<br/>The appointment is with " + appointment.TeacherFullName + " at Progress Learning Centre, Room L2-06."
                    + "<br/><br/>Student's Comment : " + appointment.StudentComment;
            }

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress("kj25133@gmail.com");

                // centennial email ...?
                mail.To.Add(appointment.StudentEmailID);
                mail.To.Add(appointment.TeacherEmailID);

                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential("kj25133@gmail.com", "Jin@_@11633477");
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }

            return body;
        }

        [HttpGet]
        public IActionResult AppointmentList(string emailId, int userId, string type)
        {
            List<Appointment> appointmentList = dynamoDB.GetAppointmentsAsync(emailId, type).Result;

            ViewBag.userId = userId;
            ViewBag.type = type;
            ViewBag.emailId = emailId;

            return View(appointmentList);
        }

        [HttpGet]
        public IActionResult SetDate(string appointmentDate, int userID, string type)
        {
            ViewBag.date = appointmentDate;

            User loginUser = usersRepository.Users.Where(u => u.UserID == userID).First();

            ViewBag.userID = loginUser.UserID;
            ViewBag.emailID = loginUser.EmailID;
            ViewBag.studentFullName = loginUser.FirstName + " " + loginUser.LastName;
            ViewBag.type = type;

            return View("AppointmentBoard", usersRepository.Users.Where(u => u.Type == "Teacher"));
            
        }

        public IActionResult CancelAppointment(string teacherEmailID, string appointmentDate_timeSlot, string emailId, int userId, string type)
        {
            Appointment appointment = dynamoDB.getAppointmentAsync(teacherEmailID, appointmentDate_timeSlot).Result;
            appointment.Status = "canceled";

            // send email
            appointment.EmailMessage = sendEmail(appointment);

            dynamoDB.CancelAppointment(teacherEmailID, appointmentDate_timeSlot, appointment.EmailMessage);

            return RedirectToAction("AppointmentList", new { emailId, userId, type });
        }
    }
}
