--Version:1.36.0.0
--Description: Allows user to have Having statements in their aggregates
if not exists(select * from sys.columns where name='HavingSQL')
begin
alter table [AggregateConfiguration]
add
HavingSQL varchar(5000) null
end