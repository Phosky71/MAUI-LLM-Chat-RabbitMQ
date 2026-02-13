# MAUI-LLM-Chat-RabbitMQ

## 📝 Descripción

Sistema de comunicación entre múltiples modelos de lenguaje (LLM) utilizando **.NET MAUI**, **RabbitMQ** y arquitectura **MVVM**. Permite que dos o más LLMs mantengan conversaciones automáticas entre sí mediante un sistema de mensajería en tiempo real.

**Tarea 4 - Integración de LLM en Local**

---

## ✨ Características

- ✅ Aplicación móvil multiplataforma con .NET MAUI
- ✅ Arquitectura MVVM completa
- ✅ Comunicación en tiempo real con RabbitMQ
- ✅ Integración con LLM local (LM Studio)
- ✅ Configuración dinámica del modelo
- ✅ Conversaciones automáticas entre LLMs
- ✅ Posibilidad de conectar con otros usuarios
- 🔜 Mejora opcional: Texto a voz (TTS)

---

## 🛠️ Tecnologías Utilizadas

| Tecnología | Versión | Propósito |
|-----------|---------|----------|
| .NET MAUI | 8.0+ | Framework multiplataforma |
| C# | 12.0 | Lenguaje principal |
| RabbitMQ | 3.x | Broker de mensajes |
| Docker | Latest | Contenedor RabbitMQ |
| LM Studio | Latest | LLM local |
| RabbitMQ.Client | 6.8+ | Cliente NuGet |
| CommunityToolkit.Mvvm | 8.2+ | MVVM Helpers |

---

## 📊 Arquitectura del Sistema

```
┌──────────────────────┐
│   App MAUI 1 (LLM1)   │
│                      │
│  [MainViewModel]      │
│  [RabbitMQ Service]   │
│  [LLM Service]        │
└────────┬─────────────┘
         │
         │  Publica/Suscribe
         │
    ┌────┴────┐
    │ RabbitMQ │
    │  Docker  │
    └────┬────┘
         │
         │
┌────────┴─────────────┐
│   App MAUI 2 (LLM2)   │
│                      │
│  [MainViewModel]      │
│  [RabbitMQ Service]   │
│  [LLM Service]        │
└──────────────────────┘
```

---

## 📁 Estructura del Proyecto

Puedes ver la estructura completa en [ESTRUCTURA_PROYECTO.md](ESTRUCTURA_PROYECTO.md)

---

## 🚀 Guía de Inicio Rápido

### 1️⃣ Prerequisitos

- **.NET 8 SDK** instalado
- **Docker Desktop** corriendo
- **LM Studio** instalado y configurado
- **Visual Studio 2022** o **VS Code** con extensión C#

### 2️⃣ Clonar el Repositorio

```bash
git clone https://github.com/Phosky71/MAUI-LLM-Chat-RabbitMQ.git
cd MAUI-LLM-Chat-RabbitMQ
```

### 3️⃣ Iniciar RabbitMQ con Docker

```bash
cd docker
docker-compose up -d
```

Verifica que RabbitMQ esté corriendo:
- **Management UI**: http://localhost:15672
- **Usuario**: admin
- **Contraseña**: admin123

### 4️⃣ Configurar LM Studio

1. Abre LM Studio
2. Descarga un modelo (recomendado: Llama 3.2 o Phi-3)
3. Inicia el servidor local en el puerto **1234**
4. Verifica que esté corriendo: http://localhost:1234/v1/models

### 5️⃣ Compilar y Ejecutar

```bash
dotnet build
dotnet run
```

---

## 💻 Comandos Útiles

### Docker

```bash
# Iniciar RabbitMQ
docker-compose up -d

# Detener RabbitMQ
docker-compose down

# Ver logs
docker-compose logs -f

# Reiniciar contenedor
docker-compose restart
```

### .NET MAUI

```bash
# Crear nuevo proyecto MAUI
dotnet new maui -n LLMChat.MAUI

# Agregar paquetes NuGet
dotnet add package RabbitMQ.Client
dotnet add package CommunityToolkit.Mvvm
dotnet add package Newtonsoft.Json

# Compilar
dotnet build

# Ejecutar en Android
dotnet build -t:Run -f net8.0-android

# Ejecutar en Windows
dotnet build -t:Run -f net8.0-windows10.0.19041.0
```

---

## 📚 Documentación Adicional

- [Estructura del Proyecto](ESTRUCTURA_PROYECTO.md)
- [Código Completo](CODIGO_COMPLETO.md)
- [Ejemplos de Uso](docs/EJEMPLOS.md)

---

## 🎯 Ejemplos de Conversación

### Debate: Gatos vs Perros

```
LLM1 (Pro-Gatos): "Los gatos son mascotas superiores por su independencia..."
LLM2 (Pro-Perros): "Los perros son compañeros más leales porque..."
```

### Juego: Tres en Raya

```
LLM1: "Coloco mi ficha en la posición central: [1,1]"
LLM2: "Respondo colocando mi ficha en: [0,0]"
```

---

## 🔧 Configuración

En la **pantalla de configuración** puedes modificar:

- **Modelo LLM**: Seleccionar qué modelo usar
- **Temperatura**: 0.0 - 1.0 (creatividad)
- **Max Tokens**: Límite de respuesta
- **System Prompt**: Personalidad del LLM
- **RabbitMQ**: Host, puerto, credenciales
- **Cola**: Nombre de la cola de mensajes

---

## ⚙️ Arquitectura MVVM

### Models
- **LLMConfig**: Configuración del modelo
- **ChatMessage**: Mensaje individual
- **RabbitMQConfig**: Parámetros de RabbitMQ

### ViewModels
- **MainViewModel**: Lógica del chat
- **ConfigViewModel**: Lógica de configuración

### Views
- **MainPage.xaml**: Interfaz principal
- **ConfigPage.xaml**: Pantalla de ajustes

### Services
- **RabbitMQService**: Comunicación con RabbitMQ
- **LLMService**: Comunicación con LM Studio

---

## 👥 Colaboradores

- **Phosky71** - Desarrollo inicial

---

## 📝 Licencia

Este proyecto es educativo y se distribuye bajo licencia MIT.

---

## 📞 Contacto

Para preguntas o sugerencias sobre este proyecto educativo, por favor abre un issue en el repositorio.

---

⚠️ **Nota**: Este es un proyecto educativo para la **Tarea 4 - Integración de LLM en Local**.
