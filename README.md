# 🚀 SmartExtractor

**SmartExtractor** es una herramienta inteligente diseñada para resolver uno de los problemas más tediosos en el manejo de datos: la extracción de tablas desde archivos PDF a Excel, potenciada por IA para permitir transformaciones personalizadas mediante lenguaje natural.

## 🌟 La Problemática

Extraer tablas de un PDF suele ser un proceso rígido que entrega datos "sucios" o mal formateados. **SmartExtractor** no solo extrae los datos, sino que permite al usuario interactuar con ellos durante el proceso mediante **Smart Prompts**.

## ✨ Características Principales

  * **Extracción Inteligente:** Identifica automáticamente estructuras de tablas en documentos complejos.
  * **Smart Prompting:** ¿Quieres que los montos se conviertan a otra moneda? ¿Que se limpie el texto de una columna? Solo pídelo con un prompt antes de exportar.
  * **Exportación Directa:** Descarga tus datos listos para usar en formato `.xlsx` (Excel).
  * **Interfaz Fluida:** Experiencia de usuario optimizada para carga rápida y previsualización.

## 🛠️ Stack Tecnológico

### Backend: .NET & FastSharp

  * **Framework:** .NET 10 / ASP.NET Core.
  * **Productividad:** Utiliza [FastSharp](https://github.com/Henry-cmd325/FastSharp) para una arquitectura de módulos rápida, limpia y altamente escalable.
  * **IA Integration:** Procesamiento de prompts para la limpieza y transformación de datos mediante LLMs.

### Frontend: Nuxt 4

  * **Framework:** Nuxt (Vue.js) para una Single Page Application (SPA) reactiva y moderna.
  * **Estilos:** Tailwind CSS para un diseño limpio y profesional.

-----

## 🚀 Instalación y Uso

### Prerrequisitos

  * [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
  * [Node.js](https://nodejs.org/) (v20+)

### Backend and Frontend

```bash
cd SmartExtractor.AppHost
dotnet restore
dotnet run
```

## 🏗️ Arquitectura

El proyecto sigue una arquitectura desacoplada:

1.  El **Frontend (Nuxt)** maneja la carga de archivos y la interacción del usuario con los prompts.
2.  El **Backend (.NET)** recibe el archivo, utiliza librerías de extracción de datos y aplica la lógica de IA configurada a través de módulos optimizados con **FastSharp**.

-----

## 👥 Autores

  * **Henry Canales** - *Desarrollador Fullstack (.NET + Nuxt)*

## 📄 Licencia

Este proyecto está bajo la Licencia MIT.