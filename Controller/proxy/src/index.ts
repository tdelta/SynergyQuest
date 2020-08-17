import express from 'express';
import httpProxy from 'http-proxy';
import https from 'https';
import fs from 'fs';
import http from 'http';

const app = express();

function errorHandler(
  err: Error,
  req: http.IncomingMessage,
  res: http.ServerResponse
) {
  console.error(err);
  res.end(`Error occured: ${err.message}`);
}

const websocketProxy = httpProxy.createProxyServer({
  target: 'ws://localhost:4242',
  changeOrigin: true,
  ws: true,
});

websocketProxy.on('error', function (err, req, res) {
  res.end('Error occurred ' + err.message);
});

const webProxy = httpProxy.createProxyServer({
  target: 'http://localhost:3000',
  changeOrigin: true,
});

webProxy.on('error', function (err, req, res) {
  res.end('Error occurred ' + err.message);
});

app.all('/*', (req, res) => {
  webProxy.web(req, res, {}, errorHandler);
});

app.on('error', parent => {
  console.log('some error.');
});

const server = https.createServer(
  {
    key: fs.readFileSync('../../Certificates/generated/server.key', 'utf8'),
    cert: fs.readFileSync('../../Certificates/generated/server.crt', 'utf8'),
  },
  app
);

server.on('upgrade', (req, socket, head) => {
  websocketProxy.ws(req, socket, head, {}, errorHandler);
});

server.on('error', err => {
  console.log('Caught flash policy server socket error: ');
  console.log(err.stack);
});

server.listen(8000);
