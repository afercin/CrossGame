CREATE DATABASE CrossGame;

USE CrossGame;

CREATE TABLE users(
	user_id INT(9) PRIMARY KEY,
	name VARCHAR(30) NOT NULL,
	number INT(4) NOT NULL,
	email VARCHAR(50) NOT NULL UNIQUE,
	password CHAR(64) NOT NULL,
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
	TCP INT(5) NOT NULL DEFAULT 3030,
	UDP INT(5) NOT NULL DEFAULT 3031,
	name VARCHAR(30),
	n_connections INT(2) NOT NULL DEFAULT 0,
	max_connections INT(2) NOT NULL DEFAULT 1,
	status INT(1) NOT NULL DEFAULT 1, -- 0 => Disconected, 1 => Connected.
	owner int(9) NOT NULL,
	FPS INT(3) NOT NULL DEFAULT 30,
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

DELIMITER //

CREATE PROCEDURE GetUserName(_email VARCHAR(30), _password CHAR(64))
BEGIN
  SELECT name, number
  FROM users
  WHERE email = _email
  AND password = _password;
END //

CREATE PROCEDURE GetUserComputers(_email VARCHAR(30), _password CHAR(64))
BEGIN
	SELECT MAC
    FROM computers
    WHERE owner = (    
		SELECT user_id
		FROM users
		WHERE email = _email
		AND password = _password);
END //

CREATE PROCEDURE GetSharedComputers(_email VARCHAR(30), _password CHAR(64))
BEGIN
	SELECT computer_id
    FROM users_computers
    WHERE user_id = (    
		SELECT user_id
		FROM users
		WHERE email = _email
		AND password = _password);
END //

CREATE PROCEDURE GetComputerData(_email VARCHAR(30), _password CHAR(64), _MAC CHAR(23))
BEGIN
	SELECT LocalIP, PublicIP, TCP, UDP, name, n_connections, max_connections, status, FPS
    FROM computers
    WHERE MAC = _MAC
    AND owner = (    
		SELECT user_id
		FROM users
		WHERE email = _email
		AND password = _password);
END //

CREATE PROCEDURE UpdateTransmissionConf(_email VARCHAR(30), _password CHAR(64), _MAC CHAR(23), _TCP INT(5), _UDP INT(5), _name VARCHAR(30), _max_connections INT(2), _FPS INT(3))
BEGIN
	UPDATE computers
    SET TCP = _TCP,
    	UDP = _UDP,
        name = _name,
        max_connections = _max_connections,
		FPS = _FPS
    WHERE MAC = _MAC
    AND owner = (    
		SELECT user_id
		FROM users
		WHERE email = _email
		AND password = _password);
END //

CREATE PROCEDURE UpdateComputerStatus(_email VARCHAR(30), _password CHAR(64), _MAC CHAR(23), _LocalIP VARCHAR(15), _PublicIP VARCHAR(15), n_connections INT(2), _status INT(1))
BEGIN
	UPDATE computers
    SET LocalIP = _LocalIP,
    	PublicIP = _PublicIP,
        status = _status
    WHERE MAC = _MAC
    AND owner = (    
		SELECT user_id
		FROM users
		WHERE email = _email
		AND password = _password);
END //

CREATE PROCEDURE GetComputerIP(_email VARCHAR(30), _password CHAR(64), _MAC CHAR(23))
BEGIN
	SELECT LocalIP, PublicIP
	FROM computers
	WHERE MAC = _MAC
    AND owner = (    
		SELECT user_id
		FROM users
		WHERE email = _email
		AND password = _password);
END //

CREATE PROCEDURE AddComputer(_email VARCHAR(30), _password CHAR(64), _MAC CHAR(23), _LocalIP VARCHAR(15), _PublicIP VARCHAR(15), _name VARCHAR(30))
BEGIN
	DECLARE temp_value INT(9);

	SELECT COUNT(MAC) INTO temp_value
	FROM computers
	WHERE MAC = _MAC;

	IF temp_value = 0 THEN        
		SELECT user_id INTO temp_value
		FROM users
		WHERE email = _email
		AND password = _password;

		INSERT INTO computers(MAC, LocalIP, PublicIP, name, owner) 
		VALUES (_MAC, _LocalIP, _PublicIP, _name, temp_value);
        
		SELECT 1 AS result;
	ELSE
		SELECT 0 AS result;
	END IF;
END //

CREATE PROCEDURE UpdateUserStatus(_email VARCHAR(30), _password CHAR(64), _status INT(1))
BEGIN
	UPDATE users
    SET status = _status
    WHERE email = _email
	AND password = _password;
END //