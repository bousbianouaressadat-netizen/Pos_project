import { useEffect, useRef, useState } from 'react';
import ProductSearch from '../../shared/components/pos/ProductSearch';
import ProductCard from '../../shared/components/pos/ProductCard';
import Cart from '../../shared/components/pos/Cart';
import PaymentModal, { PaymentMethod } from '../../shared/components/pos/PaymentModal';
import CustomerSelector, { CustomerOption } from '../../shared/components/pos/CustomerSelector';
import { CartLine, cartLineTotal } from '../../shared/components/pos/cartTypes';
import { mockCategories, mockProducts, PosProduct } from '../../shared/services/mockData/posMock';
import { usePermissions } from '../../shared/hooks/usePermissions';
import './PosPage.css';

export default function PosPage() {
  const { has } = usePermissions();
  const searchRef = useRef<HTMLInputElement>(null);

  const [search, setSearch] = useState('');
  const [activeCategory, setActiveCategory] = useState('all');
  const [lines, setLines] = useState<CartLine[]>([]);
  const [customer, setCustomer] = useState<CustomerOption | null>(null);
  const [showCustomerModal, setShowCustomerModal] = useState(false);
  const [showPaymentModal, setShowPaymentModal] = useState(false);
  const [heldSales, setHeldSales] = useState<CartLine[][]>([]);

  const filteredProducts = mockProducts.filter((p) => {
    const matchesCategory = activeCategory === 'all' || p.categoryId === activeCategory;
    const matchesSearch = p.nameAR.includes(search) || p.sku.includes(search);
    return matchesCategory && matchesSearch;
  });

  const subtotal = lines.reduce((sum, l) => sum + l.qty * l.unitPrice - l.discountAmount, 0);
  const tax = lines.reduce((sum, l) => {
    const base = l.qty * l.unitPrice - l.discountAmount;
    return sum + base * (l.taxRate / 100);
  }, 0);
  const total = subtotal + tax;

  function addProduct(product: PosProduct) {
    setLines((prev) => {
      const existing = prev.find((l) => l.productId === product.productId);
      if (existing) {
        return prev.map((l) =>
          l.productId === product.productId ? { ...l, qty: l.qty + 1 } : l
        );
      }
      return [
        ...prev,
        { productId: product.productId, nameAR: product.nameAR, unitPrice: product.price, qty: 1, discountAmount: 0, taxRate: product.taxRate },
      ];
    });
    setSearch('');
  }

  function handleBarcodeSubmit(code: string) {
    const found = mockProducts.find((p) => p.sku === code);
    if (found) addProduct(found);
  }

  function updateQty(productId: string, qty: number) {
    setLines((prev) => prev.map((l) => (l.productId === productId ? { ...l, qty } : l)));
  }

  function updateDiscount(productId: string, discount: number) {
    setLines((prev) => prev.map((l) => (l.productId === productId ? { ...l, discountAmount: discount } : l)));
  }

  function updatePrice(productId: string, price: number) {
    setLines((prev) => prev.map((l) => (l.productId === productId ? { ...l, unitPrice: price } : l)));
  }

  function removeLine(productId: string) {
    setLines((prev) => prev.filter((l) => l.productId !== productId));
  }

  function holdSale() {
    if (lines.length === 0) return;
    setHeldSales((prev) => [...prev, lines]);
    setLines([]);
    setCustomer(null);
  }

  function resumeHeldSale() {
    const last = heldSales[heldSales.length - 1];
    if (!last) return;
    setLines(last);
    setHeldSales((prev) => prev.slice(0, -1));
  }

  function cancelSale() {
    setLines([]);
    setCustomer(null);
  }

  function confirmPayment(method: PaymentMethod, paidAmount: number) {
    // TODO(API): POST /api/sales/invoices بالبيانات الحقيقية (customerId, lines, paymentMethod, paidAmount, status: "Completed")
    setShowPaymentModal(false);
    setLines([]);
    setCustomer(null);
  }

  // --- اختصارات لوحة المفاتيح ---
  useEffect(() => {
    function handleKeyDown(e: KeyboardEvent) {
      if (e.key === 'F2') { e.preventDefault(); searchRef.current?.focus(); }
      if (e.key === 'F4') { e.preventDefault(); holdSale(); }
      if (e.key === 'F5') { e.preventDefault(); setShowCustomerModal(true); }
      if (e.key === 'F8') { e.preventDefault(); if (lines.length > 0) setShowPaymentModal(true); }
      if (e.key === 'Escape') { cancelSale(); }
    }
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [lines]);

  return (
    <div className="pos-page">
      <section className="pos-page__catalog">
        <ProductSearch ref={searchRef} value={search} onChange={setSearch} onBarcodeSubmit={handleBarcodeSubmit} />

        <div className="pos-page__categories">
          {mockCategories.map((cat) => (
            <button
              key={cat.categoryId}
              className={`pos-page__category-btn ${activeCategory === cat.categoryId ? 'pos-page__category-btn--active' : ''}`}
              onClick={() => setActiveCategory(cat.categoryId)}
            >
              {cat.nameAR}
            </button>
          ))}
        </div>

        <div className="pos-page__products-grid">
          {filteredProducts.map((product) => (
            <ProductCard key={product.productId} product={product} onAdd={addProduct} />
          ))}
        </div>
      </section>

      <section className="pos-page__cart-panel">
        <button className="pos-page__customer-btn" onClick={() => setShowCustomerModal(true)}>
          {customer ? `العميل: ${customer.name}` : 'عميل نقدي (F5 لتغيير)'}
        </button>

        <Cart
          lines={lines}
          onQtyChange={updateQty}
          onDiscountChange={updateDiscount}
          onPriceChange={updatePrice}
          onRemove={removeLine}
        />

        <div className="pos-page__totals">
          <div className="pos-page__totals-row">
            <span>المجموع الفرعي</span>
            <span>{subtotal.toLocaleString()} دج</span>
          </div>
          <div className="pos-page__totals-row">
            <span>الضريبة (TVA)</span>
            <span>{tax.toLocaleString()} دج</span>
          </div>
          <div className="pos-page__totals-row pos-page__totals-row--final">
            <span>الإجمالي النهائي</span>
            <span>{total.toLocaleString()} دج</span>
          </div>
        </div>

        <div className="pos-page__actions">
          <button className="pos-page__action-btn" onClick={holdSale} disabled={lines.length === 0}>
            تعليق البيع (F4)
          </button>
          <button className="pos-page__action-btn" onClick={resumeHeldSale} disabled={heldSales.length === 0}>
            استرجاع معلّق ({heldSales.length})
          </button>
          <button className="pos-page__action-btn pos-page__action-btn--danger" onClick={cancelSale}>
            إلغاء (ESC)
          </button>
        </div>

        <button
          className="pos-page__pay-btn"
          onClick={() => setShowPaymentModal(true)}
          disabled={lines.length === 0 || !has('CanSell')}
        >
          الدفع (F8)
        </button>
      </section>

      {showCustomerModal && (
        <CustomerSelector selected={customer} onSelect={setCustomer} onClose={() => setShowCustomerModal(false)} />
      )}

      {showPaymentModal && (
        <PaymentModal
          totalAmount={total}
          hasCustomer={!!customer}
          onClose={() => setShowPaymentModal(false)}
          onConfirm={confirmPayment}
        />
      )}
    </div>
  );
}
