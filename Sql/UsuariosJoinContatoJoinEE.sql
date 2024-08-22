SELECT * FROM Usuarios AS U 
	LEFT JOIN Contatos C ON C.UsuarioId = U.Id
	LEFT JOIN EnderecosEntrega AS EE ON EE.UsuarioId = u.Id

WHERE U.Id = 1