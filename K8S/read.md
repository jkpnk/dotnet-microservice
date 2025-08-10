1. Apply platforms deployment
   `kubectl apply -f platforms-depl.yml`

2. Get deployments
   `kubectl get deployments`

3. Get Pods
   `kubectl get pods`

4. Rollout restart deployment
   `kubectl rollout restart deployment platforms-depl`

5. Get namespace
   `kubectl get namespace`

6. Get PVC
   `kubectl get pvc`

7. Get Storageclass
   `kubectl get storageclass`

8. Create secret generic mssql
   `kubectl create secret generic mssql --from-literal=SA_PASSWORD="P@ssw0rd123!"`

9. Get ingress api
   `kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.13.0/deploy/static/provider/aws/deploy.yaml`
