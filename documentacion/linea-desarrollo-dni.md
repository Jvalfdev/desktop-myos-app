# Línea de desarrollo: DNI — recorte, OCR y alta automática

Documento de referencia para la evolución del flujo de captura de documentos de identidad en **Ataena CRM**.

**Estado:** planificado (no implementado)  
**Última actualización:** 2026-06-11  
**Versión de producto de referencia:** 0.6.2

---

## Objetivo final

El estudio pide el DNI al cliente (foto por móvil, WhatsApp, o escaneo en el mostrador). Esa imagen entra en Ataena y, al terminar la línea de desarrollo:

1. Queda guardada una **foto recortada y legible** del documento.
2. La app **lee** nombre, apellidos, DNI y fecha de nacimiento (OCR).
3. **Crea el cliente** con esos datos y la foto de DNI ya asociada.

Todo **en el PC del estudio**, sin que el usuario instale programas extra ni configure API keys. Las librerías necesarias van **empaquetadas en el instalador** de Ataena.

---

## Principios

| Principio | Detalle |
|-----------|---------|
| **Offline / local** | Procesamiento en el equipo; datos del DNI no salen a la nube. Alineado con RGPD y con el modelo actual de Ataena. |
| **Sin fricción** | Solo instalar `Ataena-Setup-X.Y.Z.exe`. |
| **Por fases** | No implementar OCR ni alta automática hasta que el recorte y guardado sean fiables. |
| **Confirmación humana** | En fases 2 y 3, el usuario revisa los datos leídos antes de crear el cliente (un clic). |
| **Mismos canales de entrada** | Móvil (QR), subida desde PC y escáner WIA convergen en un único procesador de imagen. |

---

## Situación actual (v0.6.2)

- Foto de DNI ligada a un **cliente ya existente** (`FotoDniView` / `FotoDniViewModel`).
- Entrada: QR móvil (`FirmaWebService` + `photo.html`), archivo desde PC o escáner (`EscannerService`).
- La imagen se guarda **tal cual**, sin recorte ni mejora.
- Rutas: `ConsentimientoPathService.ObtenerRutaFotoDni` / `ObtenerRutaFotoDniTutor`.
- Validación manual de DNI/NIE y fecha ya existe en `ClientesViewModel` (`EsDniNieValido`, etc.).

---

## Roadmap en tres fases

```
Fase 1 ──► Fase 2 ──► Fase 3
Recorte      OCR         Alta automática
y guardado   + parseo    de cliente
```

Cada fase debe ser **usable en producción** por sí sola antes de pasar a la siguiente.

---

## Fase 1 — Recorte y guardado legible del DNI

**Meta:** que toda imagen de DNI que entre en la app se guarde como documento recortado, enderezado y con contraste adecuado.

### Alcance

- Nuevo servicio de procesado de imagen (p. ej. `DniImagenService`).
- Pipeline: imagen cruda → detección del documento → recorte + corrección de perspectiva → mejora de contraste/nitidez → JPEG final.
- Integrar en los **tres puntos de entrada** existentes, sustituyendo el guardado directo de bytes:
  - `FotoDniViewModel.EsperarFotoAsync` (móvil).
  - `FotoDniViewModel.SubirDesdeOrdenador` (PC).
  - `FotoDniViewModel.EscanearAsync` / `EscannerService` (escáner).
- Mantener flujo actual: cliente ya creado → añadir foto a ficha (esta fase no crea clientes nuevos).

### Criterios de aceptación

- [ ] Al abrir «Ver DNI», la imagen muestra **solo el documento**, sin mesa ni bordes irrelevantes en la mayoría de fotos de mesa con buena luz.
- [ ] Texto del anverso **legible** a simple vista (sin zoom extremo).
- [ ] Mismo comportamiento para DNI de cliente y de tutor (menores).
- [ ] Si el recorte automático falla: mensaje claro y opción de **reintentar** (plan B futuro: ajuste manual de 4 esquinas).
- [ ] No aumentar de forma desproporcionada el tiempo de guardado (objetivo: &lt; 3 s en PC medio).

### Tecnología candidata

- **SixLabors.ImageSharp** — ajuste de niveles, contraste, escala de grises.
- **OpenCvSharp** o detección por contornos — localizar rectángulo del documento y perspectiva.
- Todo empaquetado en el instalador (sin instalación aparte por el usuario).

### Mejora complementaria (misma fase o 1b)

- Página web móvil específica para DNI (no reutilizar `photo.html` genérico de trabajos): marco guía, cámara trasera, instrucciones («apoya el DNI en la mesa», «evita reflejos»).

### Fuera de alcance en Fase 1

- OCR.
- Alta automática de cliente.
- Validación cruzada con datos de ficha (salvo lo que ya exista).

### Release sugerido

**0.7.0** — feature visible: «Foto de DNI mejorada».

---

## Fase 2 — OCR y reconocimiento del DNI español

**Meta:** extraer de la imagen ya procesada (Fase 1) los campos del anverso del DNI/NIE español con fiabilidad aceptable.

### Alcance

- Motor OCR embebido (candidato principal: **Tesseract** + `spa.traineddata` en el instalador).
- Parser de campos para **anverso DNI/NIE**:
  - Número de documento (con validación de letra de control — reutilizar `EsDniNieValido`).
  - Nombre y apellidos.
  - Fecha de nacimiento.
- Pantalla de **revisión**: muestra imagen recortada + campos leídos; usuario confirma o corrige antes de usar los datos.
- Detección de **DNI duplicado** en BD antes de cualquier alta (reutilizar índice único en `Cliente.Dni`).

### Criterios de aceptación

- [ ] En fotos de calidad (escáner o móvil en mesa), lectura correcta del **número de DNI y fecha** en ≥ 90 % de pruebas internas.
- [ ] Nombre/apellidos ≥ 80 % en las mismas condiciones (comparación flexible: sin tildes, mayúsculas).
- [ ] Si la letra del DNI no cuadra con el número → aviso explícito, no se confía en el campo.
- [ ] Si el OCR no lee un campo obligatorio → mensaje «No se pudo leer el documento; repite la foto».
- [ ] Todo el procesamiento sigue siendo **local**.

### Formatos documentados (MVP)

| Documento | Fase 2 MVP |
|-----------|------------|
| DNI español (anverso clásico) | Sí |
| NIE (anverso) | Sí |
| DNI tarjeta / diseños recientes | Iteración posterior |
| Pasaporte | Fuera de MVP (aviso manual) |

### Fuera de alcance en Fase 2

- Crear cliente automáticamente en BD (solo pre-rellenar formulario o pantalla de borrador).
- Validación «documento vs ficha existente» (ver Fase 2b abajo).

### Release sugerido

**0.8.0** — «Lectura de DNI desde foto».

---

## Fase 2b (opcional, paralela o tras 2) — Validación al añadir foto a cliente existente

**Meta:** si el cliente **ya existe** y se añade foto de DNI, comparar lo leído con la ficha.

### Reglas de bloqueo

| Campo | Acción si no coincide |
|-------|------------------------|
| DNI | **Bloquear** guardado de foto; mensaje con valor leído vs ficha |
| Fecha de nacimiento | **Bloquear** |
| Nombre / apellidos | **Avisar**; permitir continuar si DNI y fecha coinciden |
| DNI ya usado por otro cliente | **Bloquear** |

---

## Fase 3 — Alta automática de cliente desde DNI

**Meta:** nuevo flujo «**Nuevo cliente desde DNI**» que crea el registro con foto ya guardada.

### Flujo UX

1. Usuario pulsa «Nuevo cliente desde DNI» (Dashboard o lista Clientes).
2. Elige canal: QR móvil / subir archivo / escáner.
3. Fase 1 procesa imagen → Fase 2 extrae datos.
4. Pantalla de confirmación con datos + vista previa del DNI recortado.
5. Usuario confirma → se crea `Cliente` con `FotoDniPath` y campos rellenados.
6. Navegación a ficha para RGPD, consentimientos, etc.

### Criterios de aceptación

- [ ] Cliente creado con nombre, apellidos, DNI, fecha de nacimiento y foto DNI en un flujo.
- [ ] Si DNI duplicado → no crear; ofrecer abrir cliente existente.
- [ ] Si OCR falla en campo crítico → no crear a ciegas; ofrecer alta manual o repetir foto.
- [ ] Menores: flujo tutor queda para iteración posterior o alta manual de datos de tutor tras crear al menor.

### Release sugerido

**0.9.0** o **1.0.0** según madurez del resto del producto.

---

## Arquitectura prevista (resumen)

```
┌─────────────────┐  ┌──────────────┐  ┌─────────────┐
│ Móvil (QR)      │  │ PC (archivo) │  │ Escáner WIA │
└────────┬────────┘  └──────┬───────┘  └──────┬──────┘
         │                  │                  │
         └──────────────────┼──────────────────┘
                            ▼
                 ┌──────────────────────┐
                 │  DniImagenService    │  ← Fase 1
                 │  recorte + mejora    │
                 └──────────┬───────────┘
                            ▼
                 ┌──────────────────────┐
                 │  DniOcrService       │  ← Fase 2
                 │  Tesseract + parser  │
                 └──────────┬───────────┘
                            ▼
                 ┌──────────────────────┐
                 │  Alta / validación   │  ← Fase 3 / 2b
                 │  ClientesViewModel   │
                 └──────────────────────┘
```

**Archivos actuales a tocar (referencia):**

- `Ataena/ViewModels/FotoDniViewModel.cs`
- `Ataena/Services/FirmaWebService.cs` + `wwwroot/` (página DNI móvil)
- `Ataena/Services/EscannerService.cs`
- `Ataena/Services/ConsentimientoPathService.cs`
- `Ataena/ViewModels/ClientesViewModel.cs` (alta y validaciones)
- Nuevos: `DniImagenService.cs`, `DniOcrService.cs` (nombres orientativos)

---

## Impacto en el instalador

| Componente | Tamaño orientativo |
|------------|-------------------|
| ImageSharp | ~5 MB |
| OpenCvSharp (si se usa) | +30–80 MB |
| Tesseract + `spa.traineddata` | +15–25 MB |

El instalador crecerá; es el precio de «sin instalar nada extra».

---

## RGPD y legal

- El DNI es dato de identidad; el procesamiento **local** reduce riesgo frente a APIs en la nube.
- La foto se guarda donde ya se guardan consentimientos y documentos del cliente (`%LOCALAPPDATA%\Ataena\ficheros\`).
- Conviene mencionar en textos de consentimiento / política interna que se digitaliza el DNI para gestión del estudio.
- La confirmación humana en Fase 2 y 3 evita fichas erróneas por fallo de máquina.

---

## Riesgos y mitigaciones

| Riesgo | Mitigación |
|--------|------------|
| Foto mala (reflejo, borrosa) | Marco guía en móvil; mensajes claros; reintentar |
| OCR equivocado | Validación letra DNI; pantalla de confirmación |
| Varios formatos de DNI | MVP solo anverso estándar; ampliar por iteraciones |
| Recorte falla en mano | Fase 1b: esquinas manuales |
| Instalador muy pesado | Valorar OpenCV solo si hace falta; probar primero pipeline más ligero |

---

## Checklist de seguimiento

### Fase 1 — Recorte y guardado
- [x] `DniImagenService` (OpenCvSharp: detección, perspectiva, CLAHE)
- [x] Integración en móvil, PC y escáner (`FotoDniViewModel`)
- [x] Página móvil `dni-photo.html` con guía de encuadre y ruta `/foto-dni/`
- [ ] Pruebas con 20+ fotos reales (anonimizadas) en condiciones de estudio
- [ ] Ajuste manual de esquinas si falla el recorte automático (plan B)

### Fase 2 — OCR
- [ ] Tesseract empaquetado
- [ ] Parser DNI/NIE anverso
- [ ] Pantalla de revisión de datos leídos
- [ ] Métricas de acierto en banco de pruebas

### Fase 3 — Alta automática
- [ ] Entrada «Nuevo cliente desde DNI»
- [ ] Creación atómica cliente + foto
- [ ] Manejo de duplicados
- [ ] Documentación de usuario (ayuda en app o README)

### Fase 2b — Validación ficha existente (opcional)
- [ ] Comparación DNI / fecha / nombre
- [ ] Mensajes de bloqueo por campo

---

## Historial del documento

| Fecha | Cambio |
|-------|--------|
| 2026-06-11 | Creación inicial: fases 1 (recorte), 2 (OCR), 3 (alta automática) acordadas en análisis de producto. |
