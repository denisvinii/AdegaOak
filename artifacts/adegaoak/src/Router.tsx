import { Switch, Route } from "wouter";
import { MainLayout } from "./components/layout/MainLayout";
import Dashboard from "./pages/Dashboard";
import Estoque from "./pages/Estoque";
import Movimentacoes from "./pages/Movimentacoes";
import Precos from "./pages/Precos";
import Combos from "./pages/Combos";
import VendasCombo from "./pages/VendasCombo";
import Despesas from "./pages/Despesas";
import Saldo from "./pages/Saldo";
import Funcionarios from "./pages/Funcionarios";

function Router() {
  return (
    <MainLayout>
      <Switch>
        <Route path="/" component={Dashboard} />
        <Route path="/estoque" component={Estoque} />
        <Route path="/movimentacoes" component={Movimentacoes} />
        <Route path="/precos" component={Precos} />
        <Route path="/combos" component={Combos} />
        <Route path="/vendas-combo" component={VendasCombo} />
        <Route path="/despesas" component={Despesas} />
        <Route path="/saldo" component={Saldo} />
        <Route path="/funcionarios" component={Funcionarios} />
      </Switch>
    </MainLayout>
  );
}

export default Router;
