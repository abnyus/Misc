https://www.mssqltips.com/sqlservertip/6048/sql-server-hierarchyid-data-type-overview-and-examples/

create table SimpleDemo  
(Node hierarchyid not null,  
[Geographical Name] nvarchar(30) not null,  
[Geographical Type] nvarchar(9) NULL);

insert SimpleDemo  
values
-- root level data
 ('/', 'Earth', 'Planet')

-- first level data
,('/1/','Asia','Continent')
,('/2/','Africa','Continent')
,('/3/','Oceania','Continent')

-- second level data 
,('/1/1/','China','Country')
,('/1/2/','Japan','Country')
,('/1/3/','South Korea','Country')
,('/2/1/','South Africa','Country')
,('/2/2/','Egypt','Country')
,('/3/1/','Australia','Country')

-- third level data
,('/1/1/1/','Beijing','City')
,('/1/2/1/','Tokyo','City')
,('/1/3/1/','Seoul','City')
,('/2/1/1/','Pretoria','City')
,('/2/2/1/','Cairo','City')
,('/3/1/1/','Canberra','City')

--depth-first
select 
	Node
	,Node.ToString() AS [Node Text]
	,Node.GetLevel() [Node Level]
	,[Geographical Name]
	,[Geographical Type]   
from SimpleDemo	
order by [Node Text]

--breadth-first
select 
	Node
	,Node.ToString() AS [Node Text]
	,Node.GetLevel() [Node Level]
	,[Geographical Name]
	,[Geographical Type]   
from SimpleDemo
order by [Node Level]

