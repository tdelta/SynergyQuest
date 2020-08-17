#! /bin/bash

#########
# Generates a self-signed certifcate to use with the game servers
#
# This script is based on one by Andi Wilson:
# https://gist.github.com/ptvandi/5a33c28d74ccc5100d7fe2bf5de96deb
#
# Use with react apps like this:
#
# HTTPS=true SSL_CRT_FILE=path/to/server.crt SSL_KEY_FILE=path/to/server.key yarn start
#########


CWD=$(pwd)
LOCAL_CERT_PATH=$CWD/generated

CERT_NAME='localhost (CoOpDungeon local cert)'
ORG_NAME='TeamP09'

# expire after 900 years
DAYS=328500

# -- ./.cert --

rm -rf $LOCAL_CERT_PATH
mkdir -p $LOCAL_CERT_PATH
cd $LOCAL_CERT_PATH

# -- rootCA.key --

openssl genrsa -out rootCA.key 2048

# -- rootCA.pem --

openssl req -x509 -new -nodes -key rootCA.key -subj "/C=US/ST=CA/O=$ORG_NAME/CN=$CERT_NAME" -sha256 -days $DAYS -out rootCA.pem

# -- server.csr.cnf --

cat > server.csr.cnf <<- EOM
[req]
default_bits = 2048
prompt = no
default_md = sha256
distinguished_name = dn

[dn]
C=US
ST=CA
O=$ORG_NAME
CN=$CERT_NAME
EOM

# -- v3.ext --

cat > v3.ext <<- EOM
authorityKeyIdentifier=keyid,issuer
basicConstraints=CA:FALSE
keyUsage = digitalSignature, nonRepudiation, keyEncipherment, dataEncipherment
subjectAltName = @alt_names

[alt_names]
DNS.1 = localhost
EOM

# -- server.csr & server.key --

openssl req -new -sha256 -nodes -out server.csr -newkey rsa:2048 -keyout server.key -config <( cat server.csr.cnf )

# -- server.crt & rootCA.srl --

openssl x509 -req -in server.csr -CA rootCA.pem -CAkey rootCA.key -CAcreateserial -out server.crt -days $DAYS -sha256 -extfile v3.ext

# pfx file

openssl pkcs12 -export -in server.crt -inkey server.key -out server.pfx -passout pass:
