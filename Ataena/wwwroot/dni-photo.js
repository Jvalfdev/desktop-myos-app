const canvas = document.getElementById('canvas');
const btnCamera = document.getElementById('btnCamera');
const btnUpload = document.getElementById('btnUpload');
const fileInputCamera = document.getElementById('fileInputCamera');
const fileInputGallery = document.getElementById('fileInputGallery');
const statusText = document.getElementById('status');

statusText.textContent = 'Haz una foto del anverso del DNI o elígela de la galería.';

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
    statusText.textContent = '📤 Enviando foto del DNI...';
    const response = await fetch(`/foto-dni/${token}`, {
      method: 'POST',
      headers: { 'Content-Type': 'text/plain' },
      body: dataUrl
    });

    if (response.ok) {
      statusText.textContent = '✅ DNI enviado. Ya puedes volver a Ataena en el PC.';
    } else {
      statusText.textContent = '❌ Error al enviar la foto.';
    }
  } catch (err) {
    console.error('Error al enviar la foto DNI', err);
    statusText.textContent = '❌ Error de conexión al enviar la foto.';
  }
}

function processFile(file) {
  if (!file) return;

  statusText.textContent = '📤 Procesando imagen...';
  const reader = new FileReader();
  reader.onerror = () => {
    statusText.textContent = '❌ No se pudo leer la imagen.';
  };
  reader.onload = (event) => {
    const img = new Image();
    img.onerror = () => {
      statusText.textContent = '❌ Formato de imagen no válido.';
    };
    img.onload = () => {
      const maxLado = 2000;
      let { width, height } = img;
      if (width > maxLado || height > maxLado) {
        const escala = maxLado / Math.max(width, height);
        width = Math.round(width * escala);
        height = Math.round(height * escala);
      }
      canvas.width = width;
      canvas.height = height;
      const ctx = canvas.getContext('2d');
      ctx.drawImage(img, 0, 0, width, height);
      sendPhoto(canvas.toDataURL('image/jpeg', 0.92));
    };
    img.src = event.target.result;
  };
  reader.readAsDataURL(file);
}

btnCamera.addEventListener('click', (e) => {
  e.preventDefault();
  fileInputCamera.click();
});

btnUpload.addEventListener('click', (e) => {
  e.preventDefault();
  fileInputGallery.click();
});

fileInputCamera.addEventListener('change', (e) => processFile(e.target.files[0]));
fileInputGallery.addEventListener('change', (e) => processFile(e.target.files[0]));
