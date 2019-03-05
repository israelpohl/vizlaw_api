CREATE DATABASE VIZLAW

CREATE TABLE citation_type (
id int NOT NULL,
name nvarchar(20)
)

CREATE TABLE courts (
id int,
chamber nvarchar(200),
city  nvarchar(200),
jurisdiction  nvarchar(200),
level_of_appeal  nvarchar(40),
name  nvarchar(200),
state  nvarchar(200)
)

CREATE TABLE citations (
from_id int,
to_id int,
from_case_court_id int,
from_case_date datetime ,
from_case_file_number  nvarchar(40),
from_case_private nvarchar(200),
from_case_source_name  nvarchar(200),
from_case_type  int,
from_type int,
to_case_court_jurisdiction nvarchar(200),
to_case_court_level_of_appeal nvarchar(200),
to_case_court_name nvarchar(200),
to_law_book_code nvarchar(50),
to_law_section nvarchar(20),
to_law_title nvarchar(50),
to_type int
)

CREATE TABLE courtdecisions
(
id int NOT NULL,
slug nvarchar(max),
type nvarchar(max),
file_number nvarchar(max),
create_date datetime,
date datetime,
content nvarchar(max),
court_id int
)



INSERT INTO dbo.citation_type(id, name) VALUES ( 1, 'LAW')
INSERT INTO dbo.citation_type(id, name) VALUES ( 2, 'CASE')

CREATE UNIQUE INDEX i_courtdecisions1 ON courtdecisions (id ASC); 
CREATE UNIQUE INDEX i_citations1 ON citations (from_id ASC, to_id ASC);  
CREATE UNIQUE INDEX i_courts1 ON courts (id ASC);  

CREATE FULLTEXT CATALOG courtdecisions_catalog;   
CREATE FULLTEXT INDEX ON dbo.courtdecisions  
 (   
  file_number, content    
 )   
  KEY INDEX i_courtdecisions1
      ON courtdecisions_catalog;   
