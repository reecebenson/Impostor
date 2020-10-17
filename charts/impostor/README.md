#### Install Agones

```bash
helm repo add agones https://agones.dev/chart/stable
helm install impostor-agones --namespace agones-system --create-namespace agones/agones
```

#### Install Impostor

```bash
helm install impostor-kb impostor/ --values impostor/values-master.yaml
helm install impostor-kb impostor/ --values impostor/values-node.yaml
helm install impostor-kb impostor/ --values impostor/values.yaml
```
