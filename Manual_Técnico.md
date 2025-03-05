# Manual Técnico - AutoGest Pro

## 1. Introducción

### 1.1 Propósito del Sistema
AutoGest Pro es un sistema de gestión integral para talleres de reparación de vehículos, desarrollado como parte del proyecto de Estructuras de Datos de la Universidad San Carlos de Guatemala.

### 1.2 Tecnologías Utilizadas
- **Lenguaje de Programación:** C#
- **Interfaz Gráfica:** GTK
- **Visualización de Reportes:** Graphviz
- **Plataforma:** Linux

## 2. Arquitectura del Sistema

### 2.1 Estructuras de Datos Utilizadas
El sistema implementa las siguientes estructuras de datos:

1. **Lista Simplemente Enlazada:** Gestión de Usuarios
2. **Lista Doblemente Enlazada:** Registro de Vehículos
3. **Lista Circular:** Administración de Repuestos
4. **Cola:** Gestión de Servicios
5. **Pila:** Manejo de Facturas
6. **Matriz Dispersa:** Bitácora de Relaciones Vehículo-Repuesto

### 2.2 Uso de Punteros
- Se utiliza `unsafe code` para la gestión de punteros
- Permite manejo directo de memoria para optimización de estructuras de datos

## 3. Entidades del Sistema

### 3.1 Usuarios
- **Estructura:** Lista Simplemente Enlazada
- **Atributos:**
  - ID
  - Nombres
  - Apellidos
  - Correo
  - Contraseña

### 3.2 Vehículos
- **Estructura:** Lista Doblemente Enlazada
- **Atributos:**
  - ID
  - ID_Usuario
  - Marca
  - Modelo
  - Placa

### 3.3 Repuestos
- **Estructura:** Lista Circular
- **Atributos:**
  - ID
  - Repuesto
  - Detalles
  - Costo

### 3.4 Servicios
- **Estructura:** Cola
- **Atributos:**
  - ID
  - Id_Repuesto
  - Id_Vehiculo
  - Detalles
  - Costo

### 3.5 Facturas
- **Estructura:** Pila
- **Atributos:**
  - ID
  - ID_Orden
  - Total

### 3.6 Bitácora
- **Estructura:** Matriz Dispersa
- **Función:** Registrar relación entre vehículos y repuestos utilizados

## 4. Funcionalidades del Usuario Root

### 4.1 Carga Masiva
- Soporta carga de datos desde archivos JSON
- Entidades que admiten carga masiva:
  - Usuarios
  - Vehículos
  - Repuestos

### 4.2 Ingreso Manual
Permite el registro manual de:
- Usuarios
- Vehículos
- Repuestos
- Servicios

### 4.3 Gestión de Usuarios
- Ver usuarios
- Editar usuarios
- Eliminar usuarios

### 4.4 Generación de Servicios y Facturas
- Creación automática de facturas
- Inserción de detalles en matriz dispersa
- Soporte para cancelación de facturas

## 5. Reportes

El sistema genera reportes para:
- Usuarios
- Vehículos
- Repuestos
- Servicios
- Facturación
- Bitácora
- Top 5 vehículos con más servicios
- Top 5 vehículos más antiguos

## 6. Configuración y Despliegue

### 6.1 Requisitos del Sistema
- Sistema Operativo: Linux (distribución libre)
- Entorno de Desarrollo: IDE libre
- Librería GTK para interfaces
- Graphviz para generación de reportes

### 6.2 Credenciales de Acceso
- **Usuario Administrador:** root@gmail.com
- **Contraseña:** root123

## 7. Consideraciones de Desarrollo

### 7.1 Buenas Prácticas
- Uso de punteros con `unsafe code`
- Implementación de estructuras de datos eficientes
- Validación de datos de entrada
- Manejo de errores y excepciones

### 7.2 Seguridad
- Validación de usuarios
- Gestión segura de credenciales
- Control de acceso basado en roles

## 8. Apéndices

### 8.1 Estructura de Archivos JSON para Carga Masiva
- Ejemplos incluidos en la documentación original del proyecto

## 9. Control de Versiones
- Repositorio GitHub: [EDD]1S2025_carnet
- Estructura de repositorio: Fase# dentro del mismo

**Nota:** Este manual técnico sirve como guía de referencia para desarrolladores y administradores del sistema AutoGest Pro.
