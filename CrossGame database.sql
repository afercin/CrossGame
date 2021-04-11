CREATE DATABASE CrossGame;

USE CrossGame;

CREATE TABLE users(
	user_id INT(9) PRIMARY KEY,
	name VARCHAR(30) NOT NULL,
	number INT(4) NOT NULL,
	email VARCHAR(50) NOT NULL UNIQUE,
	password CHAR(32) NOT NULL,
	status INT(1) NOT NULL, -- 0 => Disconnected, 1 => Connected, 2 => Playing, 3 => Bussy, 4 => Invisible.
	CONSTRAINT UC_Name_Number UNIQUE(name, number)
);

CREATE TABLE friendlist(
	user1 INT(9) NOT NULL,
	user2 INT(9) NOT NULL,
	accepted INT(1) NOT NULL, -- 0 => Accepted, 1 => Need user2 confirmation.
	PRIMARY KEY(user1, user2),
	CONSTRAINT FK_F_Users1 FOREIGN KEY(user1) REFERENCES users(user_id) ON DELETE CASCADE,
	CONSTRAINT FK_F_Users2 FOREIGN KEY(user2) REFERENCES users(user_id) ON DELETE CASCADE
);

CREATE TABLE computers(
	MAC CHAR(23) PRIMARY KEY,
	LocalIP VARCHAR(15) NOT NULL,
	PublicIP VARCHAR(15) NOT NULL,
	TCP INT(5) NOT NULL,
	UDP INT(5) NOT NULL,
	name VARCHAR(30),
	n_connections INT(2) NOT NULL,
	max_connections INT(2) NOT NULL,
	status INT(1) NOT NULL, -- 0 => Disconected, 1 => Connected.
	owner int(9) NOT NULL,
	CONSTRAINT FK_C_Users FOREIGN KEY(owner) REFERENCES users(user_id) ON DELETE CASCADE
);

CREATE TABLE users_computers(
	user_id int(9) NOT NULL,
	computer_id CHAR(23) NOT NULL,
	accepted INT(1) NOT NULL, -- 0 => Accepted, 1 => Need owner confirmation.
	revoke_permission DATE,
	CONSTRAINT FK_UC_Users FOREIGN KEY(user_id) REFERENCES users(user_id) ON DELETE CASCADE,
	CONSTRAINT FK_UC_Computers FOREIGN KEY(computer_id) REFERENCES computers(MAC) ON DELETE CASCADE,
	PRIMARY KEY(user_id, computer_id)
);

CREATE TABLE computers_options(
	computer_id CHAR(23) NOT NULL,
	option_name VARCHAR(30) NOT NULL,
	option_value VARCHAR(100) NOT NULL,
	CONSTRAINT FK_CO_Computers FOREIGN KEY(computer_id) REFERENCES computers(MAC) ON DELETE CASCADE,
	PRIMARY KEY(computer_id, option_name)
);

DELIMITER $$
CREATE FUNCTION random_number() RETURNS INT(4)
BEGIN
	DECLARE number INT(4);
    
    SELECT RAND() * 9000 + 1000 INTO number;
    
    RETURN number;
END $$

CREATE FUNCTION add_user(_email VARCHAR(30), _name VARCHAR(20), _password CHAR(32)) RETURNS INT(9)
BEGIN
  DECLARE return_code INT(9);
  
  SELECT COUNT(user_id) INTO return_code
  FROM users
  WHERE email = _email;
  
  IF return_code = 0 THEN
    SELECT MAX(user_id) + 1 INTO return_code FROM users;
	
	IF return_code < 2 THEN 
		SET return_code = 2; 
	END IF;
	
    INSERT INTO users VALUES(return_code, _name, random_number(), email, _password, 1);
  END IF;
  
  RETURN return_code;
END $$
