const video = document.getElementById('video');
const canvas = document.getElementById('canvas');
const btnCapture = document.getElementById('btnCapture');
const btnUpload = document.getElementById('btnUpload');
const fileInput = document.getElementById('fileInput');
const statusText = document.getElementById('status');

let stream = null;

// Debug: Log para ver qué está pasando
console.log('photo.js cargado');
console.log('fileInput:', fileInput);
console.log('btnUpload:', btnUpload);

async function initCamera() {
  if (!navigator.mediaDevices || !navigator.mediaDevices.getUserMedia) {
    statusText.textContent = '❌ Este navegador no permite usar la cámara desde esta conexión.\n\n' +
      'Prueba con otro navegador (Chrome/Firefox) o revisa los permisos de la cámara en los ajustes del navegador.';
    return;
  }

  try {
    stream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: 'environment' }, audio: false });
    video.srcObject = stream;
    statusText.textContent = 'Cámara lista. Encadra y pulsa en "Tomar foto".';
  } catch (err) {
    console.error('Error al acceder a la cámara', err);

    const ua = navigator.userAgent || '';
    const esIOS = /iPhone|iPad|iPod/i.test(ua);
    const esSafari = /Safari/i.test(ua) && !/Chrome/i.test(ua);
    const esContextoSeguro = window.isSecureContext;

    if (err.name === 'NotAllowedError' || err.name === 'PermissionDeniedError') {
      if (esIOS && esSafari && !esContextoSeguro) {
        statusText.textContent = '❌ Safari en iPhone/iPad solo permite usar la cámara en páginas seguras (https o localhost).\n\n' +
          'Esta página se está cargando desde una IP local (http), por lo que Safari bloquea la cámara aunque hayas dado permisos.\n\n' +
          '💡 Solución: Usa el botón "📁 O subir desde galería" para seleccionar una foto que ya hayas tomado con la cámara del móvil.';
      } else {
        statusText.textContent = '❌ No se ha dado permiso a la cámara.\n\n' +
          '💡 Solución rápida: Usa el botón "📁 O subir desde galería" para seleccionar una foto que ya hayas tomado.\n\n' +
          'Si quieres usar la cámara directamente, pulsa en el candado o icono de la barra de direcciones del navegador y permite el acceso a la cámara para esta página. Luego recarga la página.\n\n' +
          '(Error: ' + err.name + ')';
      }
    } else if (err.name === 'NotFoundError' || err.name === 'DevicesNotFoundError') {
      statusText.textContent = '❌ No se ha encontrado ninguna cámara en este dispositivo.\n\n' +
        '💡 Solución: Usa el botón "📁 O subir desde galería" para seleccionar una foto que ya hayas tomado.\n\n' +
        '(Error: ' + err.name + ')';
    } else {
      statusText.textContent = '❌ No se pudo acceder a la cámara.\n\n' +
        '💡 Solución: Usa el botón "📁 O subir desde galería" para seleccionar una foto que ya hayas tomado con la cámara del móvil.\n\n' +
        'Detalle técnico: ' + err.name + ' - ' + (err.message || 'sin mensaje adicional');
    }
  }
}

function getTokenFromUrl() {
  const parts = window.location.pathname.split('/');
  return parts[parts.length - 1] || '';
}

async function sendPhoto(dataUrl) {
  const token = getTokenFromUrl();
  if (!token) {
    statusText.textContent = '❌ Token no encontrado en la URL.';
    return;
  }

  try {
    statusText.textContent = '📤 Enviando foto...';
    const response = await fetch(`/foto/${token}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'text/plain'
      },
      body: dataUrl
    });

    if (response.ok) {
      statusText.textContent = '✅ Foto enviada correctamente. Ya puedes volver a la app.';
      if (stream) {
        stream.getTracks().forEach(t => t.stop());
      }
    } else {
      statusText.textContent = '❌ Error al enviar la foto.';
    }
  } catch (err) {
    console.error('Error al enviar la foto', err);
    statusText.textContent = '❌ Error al enviar la foto.';
  }
}

btnCapture.addEventListener('click', () => {
  if (!video.videoWidth || !video.videoHeight) {
    statusText.textContent = '⏳ Espera a que la cámara esté lista.';
    return;
  }

  const width = video.videoWidth;
  const height = video.videoHeight;

  canvas.width = width;
  canvas.height = height;

  const ctx = canvas.getContext('2d');
  ctx.drawImage(video, 0, 0, width, height);

  const dataUrl = canvas.toDataURL('image/jpeg', 0.9);
  sendPhoto(dataUrl);
});

// Botón para abrir galería - múltiples métodos para máxima compatibilidad
btnUpload.addEventListener('click', (e) => {
  e.preventDefault();
  e.stopPropagation();
  console.log('Botón upload clickeado');
  
  try {
    // Método 1: Click directo en el input (funciona en la mayoría de navegadores)
    fileInput.click();
    console.log('fileInput.click() ejecutado');
  } catch (err) {
    console.error('Error al hacer click en fileInput:', err);
    // Método 2: Si falla, intentar hacer el input visible momentáneamente
    statusText.textContent = '⚠️ Si no se abre la galería, toca directamente en el área del botón de nuevo.';
    
    // Forzar focus y click alternativo
    setTimeout(() => {
      fileInput.focus();
      fileInput.click();
    }, 100);
  }
});

// También permitir click directo en el input si es visible
fileInput.addEventListener('click', (e) => {
  console.log('Input file clickeado directamente');
});

fileInput.addEventListener('change', (e) => {
  console.log('Input file cambió, archivos:', e.target.files);
  const file = e.target.files[0];
  if (!file) {
    console.log('No se seleccionó ningún archivo');
    return;
  }

  console.log('Archivo seleccionado:', file.name, file.type, file.size);
  statusText.textContent = '📤 Procesando foto...';

  const reader = new FileReader();
  reader.onerror = (err) => {
    console.error('Error al leer archivo:', err);
    statusText.textContent = '❌ Error al leer la foto. Intenta con otra imagen.';
  };
  
  reader.onload = (event) => {
    console.log('Archivo leído, tamaño:', event.target.result.length);
    const img = new Image();
    img.onerror = (err) => {
      console.error('Error al cargar imagen:', err);
      statusText.textContent = '❌ Error al procesar la imagen. Asegúrate de que sea un formato válido (JPG, PNG).';
    };
    
    img.onload = () => {
      console.log('Imagen cargada, dimensiones:', img.width, 'x', img.height);
      canvas.width = img.width;
      canvas.height = img.height;
      const ctx = canvas.getContext('2d');
      ctx.drawImage(img, 0, 0);
      const dataUrl = canvas.toDataURL('image/jpeg', 0.9);
      console.log('Imagen convertida a data URL, tamaño:', dataUrl.length);
      sendPhoto(dataUrl);
    };
    img.src = event.target.result;
  };
  
  reader.readAsDataURL(file);
});

initCamera();


