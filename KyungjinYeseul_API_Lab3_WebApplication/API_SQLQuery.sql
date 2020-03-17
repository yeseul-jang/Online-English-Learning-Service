USE API_Lab3;

drop table TimeSlot;
drop table "User";

CREATE TABLE "User" (
	userID INT PRIMARY KEY NOT NULL IDENTITY(1,1),
	emailID VARCHAR(50) UNIQUE NOT NULL,
	password VARCHAR(50) NOT NULL,
	type VARCHAR(30) NOT NULL,
	firstName VARCHAR(30) NOT NULL,
	lastName VARCHAR(30) NOT NULL
);

INSERT INTO "User"(emailID, password, type, firstName, lastName) VALUES('kj25133@gmail.com', '123', 'Student', 'Kyungjin', 'Jeong');
INSERT INTO "User"(emailID, password, type, firstName, lastName) VALUES('jangysseul@gmail.com', '123', 'Teacher', 'Yeseul', 'Jang');
INSERT INTO "User"(emailID, password, type, firstName, lastName) VALUES('kyungjinca@gmail.com', '123', 'Teacher', 'Xilang', 'Gao');

CREATE TABLE TimeSlot (
	timeSlotID INT PRIMARY KEY NOT NULL IDENTITY(1,1),
	userID INT NOT NULL,
	timeSlot VARCHAR(50),
	
	CONSTRAINT FK_TimeSlot_User FOREIGN KEY (userID) REFERENCES "User"(userID)
);

INSERT INTO TimeSlot(userID, timeSlot) VALUES ('2', '10:00 ~ 11:00');
INSERT INTO TimeSlot(userID, timeSlot) VALUES ('2', '11:00 ~ 12:00');
INSERT INTO TimeSlot(userID, timeSlot) VALUES ('2', '14:00 ~ 15:00');

INSERT INTO TimeSlot(userID, timeSlot) VALUES ('3', '9:00 ~ 10:00');
INSERT INTO TimeSlot(userID, timeSlot) VALUES ('3', '15:00 ~ 16:00');