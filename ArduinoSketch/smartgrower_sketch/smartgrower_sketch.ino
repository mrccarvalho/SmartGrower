#include <SPI.h>
#include <WiFiS3.h>
#include <Wire.h>
#include "LiquidCrystal_I2C.h"
#include <DHT.h>

// Pinagem dos componentes
#define DHTPIN 4        // Pino do sensor DHT11
#define DHTTYPE DHT11   // Tipo do sensor DHT

// Pinos do Arduino
const int PINO_SENSORSOLO = A0;  // Pino conectado ao sensor de humidade do solo
const int PINO_RELE = 2;         // Pino conectado ao relé para controlar a bomba

// Variáveis globais
int humidade_solo = 0;           // Armazena a leitura da humidade do solo
// Lê a temperatura e humidade do ar
float temperatura;
float humidade_ar;
const int HUMIDADE_MINIMA_REGA = 80; // Humidade mínima para iniciar a rega (%)
unsigned long tempo_espera = 2000; // Tempo de espera entre as leituras (milissegundos)
unsigned long ultimo_tempo = 0;   // Armazena o tempo da última leitura
unsigned long ultimo_tempo_rega = 0; // Armazena o tempo da última rega

//  Wi-Fi settings
char ssid[] = "Galaxy-RC";    //  SSID da rede/
char pass[] = "ttqo3746";     //  password
int status = WL_IDLE_STATUS;  // Wifi radio's status
//  IP da aplicação
char* host = "192.168.124.243";
const int postPorta = 8082;

const long postDuracao = 10000;  //intervalo entre cada envio para a base de dados
unsigned long ultimoPost = 0;
bool conected = false;
WiFiClient client;
//---------------------
//Final definitions wifi ---------------------

// Inicia o sensor DHT
DHT dht(DHTPIN, DHTTYPE);

// Inicia o display LCD (endereço I2C, número de colunas, número de linhas)
LiquidCrystal_I2C lcd(0x27, 16, 2);  // Atualize o endereço conforme necessário

/*
 * Método WIFICONNECT para efetuar a ligação à rede
 */
void wificonnect(char ssid[], char pass[]) {
  Serial.begin(9600);  //INICIALIZA A SERIAL
  while (!Serial) {
    ;  // espera pela porta série para conectar.
  }
  delay(500);

  // ver se módulo wifi está presente:
  if (WiFi.status() == WL_NO_MODULE) {
    Serial.println("Communicação com o módulo wifi falhou!");
    while (true)
      ;
  }
  // verificar se o firmware do módulo wireless está atualizado:
  String fv = WiFi.firmwareVersion();
  if (fv < WIFI_FIRMWARE_LATEST_VERSION) {
    Serial.println("Please upgrade the firmware");
  }
  // Tentativa de ligação à Rede Wifi:
  while (status != WL_CONNECTED) {
    Serial.print("Tentativa de ligação à Rede Wifi, SSID: ");
    Serial.println(ssid);
    status = WiFi.begin(ssid, pass);
    // espera 10 segundos para tentar ligar de novo:
    delay(10000);
  }

  Serial.println(WiFi.localIP());
  // definimos a duração do último post e fazemos o post imediatamente
  // desde que o main loop inicía
  ultimoPost = postDuracao;
  Serial.println("Setup completo");
  Serial.println("");
  Serial.println("Está conectado à rede");
}

void GetLeituras() {
// Verifica se é hora de fazer outra leitura
  if (millis() - ultimo_tempo >= tempo_espera) {
    ultimo_tempo = millis();  // Atualiza o tempo da última leitura
    // Lê a humidade do solo e ajusta para uma escala de 0 a 200
    humidade_solo = analogRead(PINO_SENSORSOLO);
    humidade_solo = map(humidade_solo, 0, 1023, 0, 200);
    humidade_solo = constrain(humidade_solo, 0, 200);

    // Lê a temperatura e humidade do ar
     temperatura = dht.readTemperature();
     humidade_ar = dht.readHumidity();

    // Verifica se as leituras são válidas
    if (isnan(temperatura) || isnan(humidade_ar)) {
      Serial.println("Erro ao ler sensor DHT!");
      lcd.clear();
      lcd.setCursor(0, 1);
      lcd.print("Erro no DHT");
    } else {
      // Exibe as leituras no monitor serial
      Serial.print("Humidade do solo: ");
      Serial.print(humidade_solo);
      Serial.println("%");
      Serial.print("Temperatura: ");
      Serial.print(temperatura);
      Serial.println("*C");
      Serial.print("Humidade do ar: ");
      Serial.print(humidade_ar);
      Serial.println("%");

      // Atualiza o display LCD com as leituras
      String linha2 = "Solo:" + String(humidade_solo) + "% Temp:" + String(temperatura) + "C Ar:" + String(humidade_ar) + "%";
      scrollText(lcd, linha2, 500);  // Função para rolar o texto no LCD
    }

    // Verifica se a humidade do solo está abaixo do limite para regar
    if (humidade_solo < HUMIDADE_MINIMA_REGA) {
      // Verifica se já passou tempo suficiente desde a última rega
      if (millis() - ultimo_tempo_rega >= 30000) {
        Serial.println("Humidade abaixo do limite, ligando a bomba.");
        // Liga a bomba por 5 segundos
        digitalWrite(PINO_RELE, HIGH); // Ativa o relé para ligar a bomba
        delay(5000);  // Mantém a bomba ligada por 5 segundos
        digitalWrite(PINO_RELE, LOW);  // Desliga a bomba
        delay(15000);  // Aguarda 15 segundos antes da próxima rega
        ultimo_tempo_rega = millis();  // Atualiza o tempo da última rega
      } else {
        Serial.println("Aguardando 30 segundos após a última rega...");
        digitalWrite(PINO_RELE, LOW); // Garante que o relé está desligado
      }
    } else {
      Serial.println("Humidade acima do limite, aguardando...");
      digitalWrite(PINO_RELE, LOW); // Desliga o relé
      delay(15000);  // Aguarda 15 segundos antes da próxima verificação
      ultimo_tempo_rega = millis(); // Atualiza o tempo da última rega
    }
  }

}

void enviar_leituras() {
  Serial.begin(9600);
  Serial.println("A iniciar o envio de dados");
  Serial.print(" A conectar-se a ");
  Serial.println(host);
  Serial.print(" na porta ");
  Serial.println(postPorta);

  GetLeituras();
  // Chamar o URL/End-point as aplicação web
  String url = String("/Home/GravarLeituras?") +
  String("humidade_solo=") + String(humidade_solo) +
  String("&temperatura=") + String(temperatura) +
  String("&humidade_ar=") + String(humidade_ar);

  Serial.println("A construir o URL: ");
  Serial.print("     ");
  Serial.println(url);

  // envia o request/pedido para o servidor
  client.print(String("GET ") + url + " HTTP/1.1\r\n" + "Host: " + host + "\r\n" + "Connection: close\r\n\r\n");
  //delay(500);

  // Lê todas as linhas de resposta que vem do servidor web
  // e escreve no serial monitor
  Serial.println("Resposta do SERVIDOR: ");
  while (client.available()) {
    String line = client.readStringUntil('\r');
    Serial.print(line);
  }
  Serial.println("");
  Serial.println(" Fechar a conexão");
  Serial.println("");

  Serial.println("Dados enviados com sucesso!");
  Serial.println("");
}


void setup() {
   //chama a função para conectar ao wifi
  wificonnect(ssid, pass);
  // Inicia da comunicação serial e configuração dos pinos
  Serial.begin(9600);
  pinMode(PINO_SENSORSOLO, INPUT);  // Define o pino do sensor de humidade como entrada
  pinMode(PINO_RELE, OUTPUT);       // Define o pino do relé como saída
  digitalWrite(PINO_RELE, LOW);     // Garante que o relé inicie desligado

  // Inicia o sensor DHT
  dht.begin();

  // Configuração inicial do display LCD
  lcd.init();
  lcd.backlight();
  lcd.setCursor(0, 0);
  lcd.print("A iniciar...");
  delay(2000);  // Aguarda 2 segundos para inicializar
  lcd.clear();  // Limpa o display
}

void loop() {
  if (client.connect(host, postPorta)) {
    unsigned long diff = millis() - ultimoPost;
    if (diff > postDuracao) {
      enviar_leituras();
      ultimoPost = millis();
    }
  } else {

    Serial.println("Não foi possível conectar ao servidor!");
    delay(1000);
  }
}

// Função para rolar texto no LCD
void scrollText(LiquidCrystal_I2C &lcd, String linha, int delayTime) {
  int maxLength = linha.length();
  for (int i = 0; i < maxLength - 16 + 1; i++) {
    lcd.clear();
    lcd.setCursor(0, 1);
    lcd.print(linha.substring(i, i + 16));
    delay(delayTime);
  }
}