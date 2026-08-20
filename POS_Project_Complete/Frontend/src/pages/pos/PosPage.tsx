import { useEffect, useRef, useState } from 'react';
import ProductSearch from '../../shared/components/pos/ProductSearch';
import ProductCard from '../../shared/components/pos/ProductCard';
import Cart from '../../shared/components/pos/Cart';
import PaymentModal, { PaymentMethod } from '../../shared/components/pos/PaymentModal';
import CustomerSelector, { CustomerOption } from '../../shared/components/pos/CustomerSelector';
import { CartLine, cartLineTotal } from '../../shared/components/pos/cartTypes';
import LoadingState from '../../shared/components/data/LoadingState';
import {
  fetchPosProducts,
  fetchPosCategories,
  fetchProductByBarcode,
  PosProduct,
  PosCategory,
} from '../../shared/services/api/productsService';
import { fetchDefaultWarehouse } from '../../shared/services/api/warehousesService';
import { createSaleInvoice } from '../../shared/services/api/salesService';
import { usePermissions } from '../../shared/hooks/usePermissions';
import './PosPage.css';

export default function PosPage() {
  const { has } = usePermissions();
  const searchRef = useRef<HTMLInputElement>(null);

  const [products, setProducts] = useState<PosProduct[]>([]);
  const [categories, setCategories] = useState<PosCategory[]>([]);
  const [warehouseId, setWarehouseId] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState<string | null>(null);

  const [search, setSearch] = useState('');
  const [activeCategory, setActiveCategory] = useState('all');
  const [lines, setLines] = useState<CartLine[]>([]);
  const [customer, setCustomer] = useState<CustomerOption | null>(null);
  const [showCustomerModal, setShowCustomerModal] = useState(false);
  const [showPaymentModal, setShowPaymentModal] = useState(false);
  const [heldSales, setHeldSales] = useState<CartLine[][]>([]);
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  useEffect(() => {
    Promise.all([fetchPosProducts(), fetchPosCategories(), fetchDefaultWarehouse()])
      .then(([productsRes, categoriesRes, warehouseRes]) => {
        setProducts(productsRes);
        setCategories(categoriesRes);
        setWarehouseId(warehouseRes?.warehouseId ?? null);
      })
      .catch(() => setLoadError('تعذّر تحميل المنتجات — تحقق من رابط الـ API واتصال الخادم'))
      .finally(() => setLoading(false));
  }, []);

  const filteredProducts = products.filter((p) => {
    const matchesCategory = activeCategory === 'all' || p.categoryNameAR === activeCategory;
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
        return prev.map((l) => (l.productId === product.productId ? { ...l, qty: l.qty + 1 } : l));
      }
      return [
        ...prev,
        { productId: product.productId, nameAR: product.nameAR, unitPrice: product.price, qty: 1, discountAmount: 0, taxRate: product.taxRate },
      ];
    });
    setSearch('');
  }

  async function handleBarcodeSubmit(code: string) {
    const found = products.find((p) => p.sku === code) ?? (await fetchProductByBarcode(code));
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

  async function confirmPayment(method: PaymentMethod, paidAmount: number) {
    if (!warehouseId) {
      setSubmitError('لا يوجد مستودع افتراضي — راجع إعدادات المؤسسة');
      return;
    }

    setSubmitting(true);
    setSubmitError(null);

    try {
      await createSaleInvoice({
        customerId: customer?.customerId ?? null,
        warehouseId,
        lines,
        paymentMethod: method,
        paidAmount,
        status: 'Completed',
      });

      setShowPaymentModal(false);
      setLines([]);
      setCustomer(null);
    } catch {
      setSubmitError('فشل إنشاء الفاتورة — تحقق من الاتصال بالخادم وحاول مجددًا');
    } finally {
      setSubmitting(false);
    }
  }

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

  if (loading) return <LoadingState message="جاري تحميل نقطة البيع..." />;
  if (loadError) return <div className="pos-page__load-error">{loadError}</div>;

  return (
    <div className="pos-page">
      <section className="pos-page__catalog">
        <ProductSearch ref={searchRef} value={search} onChange={setSearch} onBarcodeSubmit={handleBarcodeSubmit} />

        <div className="pos-page__categories">
          {categories.map((cat) => (
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
            <ProductCard
              key={product.productId}
              product={{ productId: product.productId, sku: product.sku, nameAR: product.nameAR, price: product.price, taxRate: product.taxRate, categoryId: '', stock: 0, imageEmoji: '📦' }}
              onAdd={() => addProduct(product)}
            />
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

        {submitError && <p className="pos-page__submit-error">{submitError}</p>}

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

      {submitting && <div className="pos-page__submitting-overlay">جاري إنشاء الفاتورة...</div>}
    </div>
  );
}
