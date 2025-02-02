### 📌 Descrição
Este projeto é uma API desenvolvida em .NET 9 que recebe vídeos como entrada e retorna as imagens fragmentadas em frames dentro de um arquivo .zip. A API conta com integração ao MongoDB, PostgreSQL e AWS para armazenamento seguro e eficiente.

### 🔧 Configuração
- Para rodar o projeto, certifique-se que o arquivo `appsettings.json` esteja devidamente configurado na raiz do projeto da API, Worker ou Test, ex: (ToFood.ZipAPI)

![image](https://github.com/user-attachments/assets/58395996-cb27-48e3-8655-f829839a1786)


```json
{
  "// TODAS AS DEMAIS KEYS/SECRETS SÃO ACESSADAS ATRAVÉS DO SECRET MANAGER - APENAS PREENCHA SUAS CREDENCIAIS AWS ABAIXO": "",
  "AWS": {
    "SecretManager": "tofood-config",
    "AccessKey": "seu-access-key",
    "SecretKey": "seu-secret-key",
    "Region": "us-east-1"
  }
}
