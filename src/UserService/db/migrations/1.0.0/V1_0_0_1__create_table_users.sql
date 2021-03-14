create table users (
    id serial primary key,
    phone text not null,
    lastname text not null,
    firstname text not null,
    middlename text not null
);