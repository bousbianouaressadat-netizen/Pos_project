import { PosProduct } from '../../services/mockData/posMock';
import './ProductCard.css';

interface ProductCardProps {
  product: PosProduct;
  onAdd: (product: PosProduct) => void;
}

export default function ProductCard({ product, onAdd }: ProductCardProps) {
  const isLowStock = product.stock <= 10;

  return (
    <button className="product-card" onClick={() => onAdd(product)}>
      <span className="product-card__image" aria-hidden="true">{product.imageEmoji}</span>
      <span className="product-card__name">{product.nameAR}</span>
      <span className="product-card__price">{product.price.toLocaleString()} دج</span>
      <span className={`product-card__stock ${isLowStock ? 'product-card__stock--low' : ''}`}>
        المخزون: {product.stock}
      </span>
    </button>
  );
}
