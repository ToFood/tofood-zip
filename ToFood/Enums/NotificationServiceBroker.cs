using System.ComponentModel;

namespace ToFood.Domain.Enums;

public enum NotificationServiceBroker
{
    /// <summary>
    /// Disparo Pro
    /// </summary>
    [Description("Disparo Pro")]
    DisparoPro = 1,

    /// <summary>
    /// Voa MVNO
    /// </summary>
    [Description("Voa (MVNO)")]
    VoaMvno = 2,

    /// <summary>
    /// Zenvia
    /// </summary>
    [Description("Zenvia")]
    Zenvia = 3,

    /// <summary>
    /// NeWave
    /// </summary>
    [Description("NeWave")]
    NeWave = 4,

    /// <summary>
    /// Evotrix
    /// </summary>
    [Description("Evotrix")]
    Evotrix = 5,

    /// <summary>
    /// SMTP
    /// </summary>
    [Description("SMTP")]
    Smtp = 6,

    /// <summary>
    /// SMTP inseguro
    /// </summary>
    [Description("SMTP inseguro")]
    SmtpInsecure = 7,

    /// <summary>
    /// Volare
    /// </summary>
    [Description("Volare")]
    Volare = 8,

    /// <summary>
    /// Facilita
    /// </summary>
    [Description("Facilita")]
    Facilita = 9,

    /// <summary>
    /// Eai
    /// </summary>
    [Description("Eaí")]
    Eai = 10,

    /// <summary>
    /// AWS
    /// </summary>
    [Description("AWS")]
    Aws = 11,

    /// <summary>
    /// SMS Gestor
    /// </summary>
    [Description("SMS Gestor")]
    SmsGestor = 12,

    /// <summary>
    /// HotMobile
    /// </summary>
    [Description("HotMobile")]
    HotMobile = 13,

    /// <summary>
    /// Zapisp
    /// </summary>
    [Description("Zapisp")]
    Zapisp = 14,

    /// <summary>
    /// Chatmix
    /// </summary>
    [Description("Chatmix")]
    Chatmix = 15,

    /// <summary>
    /// Twilio
    /// </summary>
    [Description("Twilio")]
    Twilio = 16,

    /// <summary>
    /// DirectCall
    /// </summary>
    [Description("DirectCall")]
    DirectCall = 17,

    /// <summary>
    /// Gupshup
    /// </summary>
    [Description("Gupshup")]
    Gupshup = 18,

    /// <summary>
    /// AlertZ (MMCenter)
    /// </summary>
    [Description("AlertZ (MMCenter)")]
    AlertZ = 19,

    /// <summary>
    /// MegaDedicados
    /// </summary>
    [Description("MegaDedicados")]
    MegaDedicados = 20,

    /// <summary>
    /// SMS Solution
    /// </summary>
    [Description("SMS Solution")]
    SmsSolution = 21,

    /// <summary>
    /// Super Way
    /// </summary>
    [Description("Super Way")]
    SuperWay = 22,

    /// <summary>
    /// Matrix
    /// </summary>
    [Description("Matrix")]
    Matrix = 23,

    /// <summary>
    /// James
    /// </summary>
    [Description("James")]
    James = 24,

    /// <summary>
    /// KingSms
    /// </summary>
    [Description("KingSms")]
    KingSms = 25,

    /// <summary>
    /// 360 Dialog
    /// </summary>
    [Description("360 Dialog")]
    Dialog360 = 26,

    /// <summary>
    /// Conesul
    /// </summary>
    [Description("Rede Conesul (MVNO)")]
    Conesul = 27,

    /// <summary>
    /// 7Pro
    /// </summary>
    [Description("7Pro")]
    Pro7 = 28,

    /// <summary>
    /// SzChat
    /// </summary>
    [Description("SzChat")]
    SzChat = 29,

    /// <summary>
    /// Chat2Desk
    /// </summary>
    [Description("Chat2Desk")]
    Chat2Desk = 30,

    /// <summary>
    /// SuperWhats
    /// </summary>
    [Description("SuperWhats")]
    SuperWhats = 31,

    /// <summary>
    /// ZenviaV2
    /// </summary>
    [Description("ZenviaV2")]
    ZenviaV2 = 32,

    /// <summary>
    /// CapitalMidia
    /// </summary>
    [Description("Capital Midia")]
    CapitalMidia = 33,

    /// <summary>
    /// SocialHub Whatsapp
    /// </summary>
    [Description("Social Hub")]
    SocialHub = 34,

    /// <summary>
    /// Iungo Whatsapp
    /// </summary>
    [Description("Iungo")]
    Iungo = 35,

    /// <summary>
    /// Sinch Whatsapp
    /// </summary>
    [Description("Sinch")]
    Sinch = 36,

    /// <summary>
    /// Blip Whatsapp
    /// </summary>
    [Description("Blip")]
    Blip = 37,

    /// <summary>
    /// Firebase Notification
    /// </summary>
    [Description("FirebaseNotification")]
    FirebaseNotification = 38,

    /// <summary>
    /// ID Soluções Web
    /// </summary>
    [Description("ID Soluções Web")]
    IDSolucoes = 39,

    /// <summary>
    /// 7AZ WhatsApp
    /// </summary>
    [Description("7AZ WhatsApp")]
    SeteAzWhatsApp = 40,

    /// <summary>
    /// SendGrid
    /// </summary>
    [Description("SendGrid Email API")]
    SendGridEmailApi = 41,

    /// <summary>
    /// Serviço Genérico
    /// </summary>
    [Description("Serviço Genérico")]
    GenericService = 42,

    /// <summary>
    /// Serviço Genérico
    /// </summary>
    [Description("Zeus")]
    Zeus = 43,

    /// <summary>
    /// Zenvia WhatsApp
    /// </summary>
    [Description("Zenvia WhatsApp")]
    ZenviaWhatsapp = 44,

    /// <summary>
    /// Zenvia WhatsApp
    /// </summary>
    [Description("Tip")]
    Tip = 45,

    /// <summary>
    /// SUMMIT
    /// </summary>
    [Description("Summit")]
    Summit = 46,

    /// <summary>
    /// AloChat
    /// </summary>
    [Description("AloChat")]
    AloChat = 47,

    /// <summary>
    /// Droyds
    /// </summary>
    [Description("Droyds")]
    Droyds = 48,

    /// <summary>
    /// YupChat
    /// </summary>
    [Description("YupChat")]
    YupChat = 49,

    /// <summary>
    /// Infobip
    /// </summary>
    [Description("Infobip")]
    Infobip = 50,

    /// <summary>
    /// MatrixV2
    /// </summary>
    [Description("MatrixV2")]
    MatrixV2 = 51,

    /// <summary>
    /// 360 Dialog v2
    /// </summary>
    [Description("360V2")]
    Dialog360V2 = 52,

    /// <summary>
    /// Voanet
    /// </summary>
    [Description("Voanet")]
    Voanet = 53,

    /// <summary>
    /// ChatmixV2
    /// </summary>
    [Description("ChatmixV2")]
    ChatmixV2 = 54,

    /// <summary>
    /// Zendesk
    /// </summary>
    [Description("Zendesk")]
    Zendesk = 55,

    /// <summary>
    /// Paula - IA Bemobi
    /// </summary>
    [Description("Paula")]
    Paula = 56,

    /// <summary>
    /// MatrixPay
    /// </summary>
    [Description("MatrixPay")]
    MatrixPay = 57,

    /// <summary>
    /// InfobipSms
    /// </summary>
    [Description("InfobipSms")]
    InfobipSms = 58
}
