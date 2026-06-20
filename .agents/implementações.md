## Ajustes

- ao carregar os parâmetros na tela inicial parece ter um problema de concorrência ou algo do tipo, as informações estão demorando para serem exibidas, as infos deveriam carregar junto da home, talvez tornar as tasks sincronas ao invés de assincronas.
- ajustar o login do usuario, atualmente se o usuario digita uma senha errada ou o usuario nao existe na base de dados, ele é apenas redirecionado para a home do sistema, sem nenhum aviso ou alerta do erro verdadeiro. ao errar a senha ou algo do tipo o usuario deve permanecer na tela de login e deve ser exibida uma notificacao em vermelho dizendo o erro que ocorreu.

## Novas funcionalidades

- criar um perfil para o usuário tendo a possibilidade dele adicionar uma foto de perfil (com formatos png, jpg e jpeg) e ter a possibilidade de criar horários fixos que se repetem a cada x semanas, por exemplo, o usuario seleciona o serviço, o dia da semana, o horário do dia e a cada semana ele vai realizar aquele mesmo agendamento e ai o sistema lança a solicitação dos agendamento todo domingo de noite.

- criar uma lógica que cancela as requisições anteriores ao fazer muitas requests iguais, por exemplo, se o usuario clicar muitas vezes sobre certa data, ele lança multiplas requests para o app verificar a disponibilidade, o que pode derrubar o banco. se 10 requisicoes iguais forem  feitas em seguida no navegador do usuario, deve ser mantida apenas a ultima request.