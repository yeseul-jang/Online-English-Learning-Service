using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KyungjinYeseul_API_Lab3_WebApplication.Models
{
    public class Appointment_DBOperation
    {
        AmazonDynamoDBClient client;
        DynamoDBContext context;
        BasicAWSCredentials credentials;
        Table userTable;

        public Appointment_DBOperation(string tableName)
        {
            credentials = new BasicAWSCredentials("AKIAVC2QJKV2O3OA32J5", "uFsz+kRG9CzEcIe6LhIzLp/o6pqdLLpt6guLUtKR");
            client = new AmazonDynamoDBClient(credentials, Amazon.RegionEndpoint.CACentral1);
            //context = new DynamoDBContext(client);
            userTable = Table.LoadTable(client, tableName, DynamoDBEntryConversion.V2);
        }

        public void CreateTable()
        {
            CreateTableRequest request = new CreateTableRequest
            {
                TableName = "Appointment",
                AttributeDefinitions = new List<AttributeDefinition>
                {
                    new AttributeDefinition
                    {
                        AttributeName="teacherEmailID",
                        AttributeType="S"
                    },
                     new AttributeDefinition
                    {
                        AttributeName="appointmentDate_timeSlot",
                        AttributeType="S"
                    }
                },
                KeySchema = new List<KeySchemaElement>
                {
                    new KeySchemaElement
                    {
                        AttributeName="teacherEmailID",
                        KeyType="HASH"
                    },
                    new KeySchemaElement
                    {
                        AttributeName="appointmentDate_timeSlot",
                        KeyType="RANGE"
                    }
                },
                BillingMode = BillingMode.PROVISIONED,
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = 2,
                    WriteCapacityUnits = 1
                }

            };

            var response = client.CreateTableAsync(request);

        }

        public async Task InsertAppointmentAsync(Appointment appointment)
        {
            string tempStudentComment;
            string tempTeacherComment;

            if(appointment.StudentComment == null)
            {
                tempStudentComment = "";
            }else
            {
                tempStudentComment = appointment.StudentComment;
            }

            if (appointment.TeacherComment == null)
            {
                tempTeacherComment = "";
            }
            else
            {
                tempTeacherComment = appointment.TeacherComment;
            }

            Document newUser = new Document
            {
                ["teacherEmailID"] = appointment.TeacherEmailID,
                ["appointmentDate_timeSlot"] = appointment.AppointmentDate_TimeSlot,
                ["studentEmailID"] = appointment.StudentEmailID,
                ["studentFullName"] = appointment.StudentFullName,
                ["teacherFullName"] = appointment.TeacherFullName,
                ["course"] = appointment.Course,
                ["emailMessage"] = appointment.EmailMessage,
                ["studentComment"] = tempStudentComment,
                ["teacherComment"] = tempTeacherComment,
                ["status"] = appointment.Status
            };


            await userTable.PutItemAsync(newUser);
        }

        public async Task<List<Appointment>> GetAppointmentsAsync(string emailID, string type)
        {
            Table ThreadTable = Table.LoadTable(client, "Appointment");

            ScanFilter scanFilter = new ScanFilter();

            string emailCondition = "";
            if (type == "Teacher")
                emailCondition = "teacherEmailID";
            else
                emailCondition = "studentEmailID";

            scanFilter.AddCondition(emailCondition, ScanOperator.Equal, emailID);

            Search search = ThreadTable.Scan(scanFilter);

            List<Document> documentList = new List<Document>();
            List<Appointment> appointmentList = new List<Appointment>();

            do
            {
                documentList = await search.GetNextSetAsync();
                foreach (var document in documentList)
                {
                    Appointment newAppointment = new Appointment();

                    foreach (var attribute in document.GetAttributeNames())
                    {
                        string stringValue = null;
                        var value = document[attribute];

                        stringValue = value.AsPrimitive().Value.ToString();

                        if ("teacherEmailID".Equals(attribute))
                        {
                            newAppointment.TeacherEmailID = stringValue;
                        }
                        else if ("appointmentDate_timeSlot".Equals(attribute))
                        {
                            newAppointment.AppointmentDate_TimeSlot = stringValue;
                        }
                        else if ("studentEmailID".Equals(attribute))
                        {
                            newAppointment.StudentEmailID = stringValue;
                        }
                        else if ("studentFullName".Equals(attribute))
                        {
                            newAppointment.StudentFullName = stringValue;
                        }
                        else if ("teacherFullName".Equals(attribute))
                        {
                            newAppointment.TeacherFullName = stringValue;
                        }
                        else if ("course".Equals(attribute))
                        {
                            newAppointment.Course = stringValue;
                        }
                        else if ("emailMessage".Equals(attribute))
                        {
                            newAppointment.EmailMessage = stringValue;
                        }
                        else if ("studentComment".Equals(attribute))
                        {
                            newAppointment.StudentComment = stringValue;
                        }
                        else if ("teacherComment".Equals(attribute))
                        {
                            newAppointment.TeacherComment = stringValue;
                        }
                        else if ("status".Equals(attribute))
                        {
                            newAppointment.Status = stringValue;
                        }
                    }

                    // how to check from query? how to get unique??
                    //if(!bookList.Contains(newBook))
                    appointmentList.Add(newAppointment);
                }
            } while (!search.IsDone);

            return appointmentList;
        }

        public async Task<Appointment> getAppointmentAsync(string teacherEmailID, string appointmentDate_timeSlot)
        {
            Appointment newAppointment = new Appointment();

            Table ThreadTable = Table.LoadTable(client, "Appointment");

            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("teacherEmailID", ScanOperator.Equal, teacherEmailID);
            scanFilter.AddCondition("appointmentDate_timeSlot", ScanOperator.Equal, appointmentDate_timeSlot);
            Search search = ThreadTable.Scan(scanFilter);

            var documents = await search.GetRemainingAsync();

            foreach (var document in documents)
            {
                foreach (var attribute in document.GetAttributeNames())
                {
                    string stringValue = null;
                    var value = document[attribute];

                    stringValue = value.AsPrimitive().Value.ToString();

                    if ("teacherEmailID".Equals(attribute))
                    {
                        newAppointment.TeacherEmailID = stringValue;
                    }
                    else if ("appointmentDate_timeSlot".Equals(attribute))
                    {
                        newAppointment.AppointmentDate_TimeSlot = stringValue;
                    }
                    else if ("studentEmailID".Equals(attribute))
                    {
                        newAppointment.StudentEmailID = stringValue;
                    }
                    else if ("studentFullName".Equals(attribute))
                    {
                        newAppointment.StudentFullName = stringValue;
                    }
                    else if ("teacherFullName".Equals(attribute))
                    {
                        newAppointment.TeacherFullName = stringValue;
                    }
                    else if ("course".Equals(attribute))
                    {
                        newAppointment.Course = stringValue;
                    }
                    else if ("emailMessage".Equals(attribute))
                    {
                        newAppointment.EmailMessage = stringValue;
                    }
                    else if ("studentComment".Equals(attribute))
                    {
                        newAppointment.StudentComment = stringValue;
                    }
                    else if ("teacherComment".Equals(attribute))
                    {
                        newAppointment.TeacherComment = stringValue;
                    }
                    else if ("status".Equals(attribute))
                    {
                        newAppointment.Status = stringValue;
                    }
                }
            }

            return newAppointment;
        }

        public async Task<bool> isBookedAsync(string teacherEmail, string date_timeslot)
        {
            Appointment newAppointment = new Appointment();

            Table ThreadTable = Table.LoadTable(client, "Appointment");

            ScanFilter scanFilter = new ScanFilter();
            scanFilter.AddCondition("teacherEmailID", ScanOperator.Equal, teacherEmail);
            scanFilter.AddCondition("appointmentDate_timeSlot", ScanOperator.Equal, date_timeslot);
            scanFilter.AddCondition("status", ScanOperator.Equal, "made");
            Search search = ThreadTable.Scan(scanFilter);

            var documents = await search.GetRemainingAsync();

            if(documents.Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void CancelAppointment(string teacherEmailID, string date_time, string emailMessage)
        {
            Table table = Table.LoadTable(client, "Appointment");

            var appointment = new Document();

            // Set the attributes that you wish to update.
            appointment["teacherEmailID"] = teacherEmailID; // Primary key.
                                                            // Replace the authors attribute.
            appointment["appointmentDate_timeSlot"] = date_time;

            // change status to canceled
            appointment["status"] = "canceled";
            appointment["emailMessage"] = emailMessage;

            table.UpdateItemAsync(appointment);
        }
    }
}

