# Pathological Authentication

It's a HTTP service dedicated for users and claims management and for authentication.

For now only Basic Authentication is supported. Later maybe I will add some JWT. And no cookies. Sugar is not healthy.

## Installation

Docker: https://hub.docker.com/repository/docker/dzaba/patho-autho

`docker run -d --name patho-autho-container -e PATHOAUTHO_CONNECTION_STRING="YOUR_SQL_SERVER_CONNECTION_STRING" -e ASPNETCORE_HTTP_PORTS=80 -p 8080:80 dzaba/patho-autho`

Database is created on first start.

## Usage

Check swagger. Example: http://localhost:8080/swagger

There's a super admin user created while making the database. Login: admin@test.com, password: Password1!

It's a good idea first to make your own user calling the `POST /User` method.
Then upgrade it to super user by calling the `POST /Role/identity/SuperAdmin/user/YOUR_USER`.

Then you can delete the admin@test.com by calling the `DELETE /User/admin@test.com` method.

The next thing which probably you want to do is to make some new application by calling the `POST /Application/YOUR_APP` method.
It will return the GUID ID for your new application.

You can setup admins of your application by calling the `POST /Application/YOUR_APP_ID/admin/user/SOME_USER`.
Only super admins or dedicated application admins can change it.

### Roles and claims

You can make application roles by calling `POST /Role`

You can assign user to roles by calling `POST /Role/{roleId}/user/{userName}`

You can assign claims to users by calling `POST /Claim`

### Authentication

Use the `GET /User/current/application/{appId}` method in your application for getting every roles and claims for any user.

If HTTP 401 code then credentials are bad.

## Client libraries

- [C#](src/Dzaba.PathoAutho.Client)