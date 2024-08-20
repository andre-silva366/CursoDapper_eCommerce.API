SELECT Nome,Email,Sexo,RG,CPF,NomeMae,SituacaoCadastro, DataCadastro, c.Telefone, c.Celular
FROM Usuarios as u LEFT JOIN Contatos AS c ON u.Id = c.UsuarioId WHERE U.Id = 1;